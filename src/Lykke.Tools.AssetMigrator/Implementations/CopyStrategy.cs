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
    public class CopyStrategy : ICopyStrategy
    {
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _meClient;
        private readonly IMigrationRepository _migrationRepository;
        private readonly ICopyOptions _options;
        
        public CopyStrategy(
            IBalanceService balanceService,
            ILogFactory logFactory,
            IMatchingEngineClient meClient,
            IMigrationRepository migrationRepository,
            ICopyOptions options)
        {
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
            _meClient = meClient;
            _migrationRepository = migrationRepository;
            _options = options;
        }
        
        public async Task ExecuteAsync()
        {
            _log.Info("Copying started.");

            var balances = await _balanceService.GetBalancesAsync();
            
            _log.Info($"Got {balances.Length} balances to copy.");

            await CopyAsync(balances);
            
            _log.Info("Copying completed.");
        }
        
        private async Task CopyAsync(
            IReadOnlyList<BalanceEntity> balances)
        {
            for (var i = 0; i < balances.Count; i++)
            {
                var balance = balances[i];

                try
                {
                    var cashInId = await _migrationRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.TargetAssetId);
                    var cashInAmount = ((double) balance.Balance * _options.Multiplier).TruncateDecimalPlaces((int) _options.TargetAssetAccuracy);
                
                    var cashInResult = await _meClient.CashInOutAsync
                    (
                        id: cashInId.ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.TargetAssetId,
                        amount: cashInAmount
                    );
                
                    if (cashInResult.Status != MeStatusCodes.Ok && cashInResult.Status != MeStatusCodes.Duplicate)
                    {
                        _log.Warning($"CashIn [{cashInAmount}] for client [{balance.ClientId}] completed with [{cashInResult.Status.ToString()}] status.");
                    }
                    
                    _log.Info($"Copied {i + 1} of {balances.Count} balances.");
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed to copy balance [{balance.Balance}] for client [{balance.ClientId}].");
                }
            }
        }
    }
}
