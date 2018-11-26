using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnStrategy : IBurnStrategy
    {
        private readonly IBalanceRepository _balanceRepository;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _meClient;
        private readonly IBurnOptions _options;

        public BurnStrategy(
            IBalanceRepository balanceRepository,
            IBurnOptions options,
            ILogFactory logFactory,
            IMatchingEngineClient meClient)
        {
            _balanceRepository = balanceRepository;
            _log = logFactory.CreateLog(this);
            _meClient = meClient;
            _options = options;
        }

        
        public async Task ExecuteAsync()
        {
            _log.Info($"Balance burning started for client {_options.ClientId}");
            
            var balance = await _balanceRepository.TryGetBalanceAsync
            (
                clientId: _options.ClientId,
                assetId: _options.AssetId
            );

            var burnAmount = balance != null
                ? ((double) balance.Balance).TruncateDecimalPlaces((int) _options.AssetAccuracy) * -1
                : 0;
            
            if (burnAmount < 0)
            {
                var burnResult = await _meClient.CashInOutAsync
                (
                    id: Guid.NewGuid().ToString(),
                    clientId: _options.ClientId,
                    assetId: _options.AssetId,
                    amount: burnAmount
                );

                if (burnResult.Status != MeStatusCodes.Ok)
                {
                    _log.Warning($"CashOut [{burnAmount}] for client [{_options.ClientId}] completed with [{burnResult.Status.ToString()}] status.");
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    _log.Info($"Balance [{balance.Balance}] burning completed");
                }
            }
            else
            {
                _log.Warning("Balance not found");
            }
        }
    }
}
