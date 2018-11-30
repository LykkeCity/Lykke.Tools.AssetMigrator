using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Tools.AssetMigrator.Entities;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnStrategy : IBurnStrategy
    {
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _meClient;
        private readonly IBurnOptions _options;

        public BurnStrategy(
            IBalanceService balanceService,
            IBurnOptions options,
            ILogFactory logFactory,
            IMatchingEngineClient meClient)
        {
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
            _meClient = meClient;
            _options = options;
        }

        
        public async Task ExecuteAsync()
        {
            _log.Info("Burning started.");

            var balances = await _balanceService.GetBalancesAsync();
            
            _log.Info($"Got {balances.Length} balances to burn.");

            await BurnAsync(balances);
            
            _log.Info("Burning completed.");
        }

        private async Task BurnAsync(
            IReadOnlyList<BalanceEntity> balances)
        {
            for (var i = 0; i < balances.Count; i++)
            {
                var balance = balances[i];

                try
                {
                    var burnAmount = ((double) balance.Balance).TruncateDecimalPlaces((int) _options.AssetAccuracy) * -1;

                    var burnResult = await _meClient.CashInOutAsync
                    (
                        id: Guid.NewGuid().ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.AssetId,
                        amount: burnAmount
                    );
                    
                    if (burnResult.Status != MeStatusCodes.Ok)
                    {
                        _log.Warning($"CashOut [{burnAmount}] for client [{balance.ClientId}] completed with [{burnResult.Status.ToString()}] status.");
                    }
                    
                    _log.Info($"Burned {i + 1} of {balances.Count} balances.");
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed to copy balance [{balance.Balance}] for client [{balance.ClientId}].");
                }
            }
        }
    }
}
