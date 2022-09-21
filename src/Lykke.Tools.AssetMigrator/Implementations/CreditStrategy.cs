using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CreditStrategy : ICreditStrategy
    {
        private ILog _log;
        private readonly IMatchingEngineClient _meClient;
        private readonly IMigrationRepository _migrationRepository;
        private readonly ICreditOptions _options;

        public CreditStrategy(ILogFactory logFactory,
            IMatchingEngineClient meClient,
            IMigrationRepository migrationRepository,
            ICreditOptions options)
        {
            _log = logFactory.CreateLog(this);
            _meClient = meClient;
            _migrationRepository = migrationRepository;
            _options = options;
        }

        public async Task ExecuteAsync()
        {
            _log.Info("Crediting started.");

            await Creadit();

            _log.Info("Crediting completed");
        }

        private async Task Creadit()
        {
            _log.Info($"Reading credit plan file {_options.CreditPlanCsvPath}");

            using var walletsFile = File.OpenText(_options.CreditPlanCsvPath);

            // Reads the header
            await walletsFile.ReadLineAsync();

            var linesRead = 1;
            var creditedWallets = 0;

            while (!walletsFile.EndOfStream)
            {
                var line = await walletsFile.ReadLineAsync();

                ++linesRead;

                if (line == null)
                {
                    continue;
                }

                var cells = line.Split(',');

                if (cells.Length < 4)
                {
                    _log.Warning($"Unexpected cells count [{cells.Length}] in the CSV file line [{linesRead}]: [{line}] ");
                    continue;
                }

                var walletId = cells[0];
                var assetId = cells[1];
                var assetAccuracy = int.Parse(cells[2], CultureInfo.InvariantCulture);
                var amount = decimal.Parse(cells[3], CultureInfo.InvariantCulture);

                try
                {
                    var cashInId = await _migrationRepository.GetOrCreateCashInOutIdAsync(walletId, assetId);
                    var cashInAmount = ((double)amount).TruncateDecimalPlaces(assetAccuracy);

                    var cashInResult = await _meClient.CashInOutAsync
                    (
                        id: cashInId.ToString(),
                        clientId: walletId,
                        assetId: assetId,
                        amount: cashInAmount
                    );

                    if (cashInResult.Status != MeStatusCodes.Ok && cashInResult.Status != MeStatusCodes.Duplicate)
                    {
                        _log.Warning($"CashIn [{cashInAmount}] for wallet [{walletId}] completed with unexpected status: [{cashInResult.Status}]");
                    }

                    creditedWallets++;

                    _log.Info($"{creditedWallets} Credits completed");
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed to credit wallet [{walletId}] with [{amount}] of asset [{assetId}]");
                }
            }

            walletsFile.Close();
        }
    }
}
