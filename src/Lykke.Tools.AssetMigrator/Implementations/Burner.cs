using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Services;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class Burner : IBurner
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly IBurnOptions _options;

        public Burner(
            IBurnOptions options,
            ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }

        
        public async Task RunAsync()
        {
            _log.Info("Balance burning started");
            
            var meClient = new TcpMatchingEngineClient(_options.MEEndPoint, EmptyLogFactory.Instance);

            var balanceRepository = new BalanceRepository
            (
                _options.BalancesConnectionString,
                _logFactory
            );
            
            meClient.Start();

            var balance = await balanceRepository.TryGetBalanceAsync
            (
                clientId: _options.ClientId,
                assetId: _options.AssetId
            );

            var burnAmount = balance != null
                ? ((double) balance.Balance).TruncateDecimalPlaces((int) _options.AssetAccuracy) * -1
                : 0;
            
            if (burnAmount < 0)
            {
                var burnResult = await meClient.CashInOutAsync
                (
                    id: Guid.NewGuid().ToString(),
                    clientId: _options.ClientId,
                    assetId: _options.AssetId,
                    amount: burnAmount
                );

                if (burnResult.Status != MeStatusCodes.Ok)
                {
                    _log.Warning($"CashOut for client [{_options.ClientId}] completed with [{burnResult.Status.ToString()}] status.");
                }
            }
            else
            {
                _log.Warning("Balance not found");
            }
            
            _log.Info("Balance burning completed");
        }
    }
}
