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
    public class TransferStrategy : ITransferStrategy
    {
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _meClient;
        private readonly IMigrationRepository _migrationRepository;
        private readonly IMigrateOptions _options;
        
        
        public TransferStrategy(
            IBalanceService balanceService,
            ILogFactory logFactory,
            IMatchingEngineClient meClient,
            IMigrationRepository migrationRepository,
            IMigrateOptions options)
        {
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
            _meClient = meClient;
            _migrationRepository = migrationRepository;
            _options = options;
        }
        
        public async Task ExecuteAsync()
        {
            _log.Info("Transfer started.");

            var balances = await _balanceService.GetBalancesAsync();
            
            _log.Info($"Got {balances.Length} balances to transfer.");

            await TransferAsync(balances);
            
            _log.Info("transfer completed.");
        }
        
        private async Task TransferAsync(
            IReadOnlyList<BalanceEntity> balances)
        {
            for (var i = 0; i < balances.Count; i++)
            {
                var balance = balances[i];
                
                try
                {
                    var cashInId = await _migrationRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.TargetAssetId);
                    var cashOutId = await _migrationRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.SourceAssetId);
                    var cashInAmount = ((double) balance.Balance * _options.Multiplier).TruncateDecimalPlaces((int) _options.TargetAssetAccuracy);
                    var cashOutAmount = (double) balance.Balance * -1;
                    
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

                    var cashOutResult = await _meClient.CashInOutAsync
                    (
                        id: cashOutId.ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.SourceAssetId,
                        amount: cashOutAmount
                    );

                    if (cashOutResult.Status != MeStatusCodes.Ok || cashOutResult.Status != MeStatusCodes.Duplicate)
                    {
                        _log.Warning($"CashOut [{cashOutAmount}] completed for client {balance.ClientId} with [{cashOutResult.Status.ToString()}] status.");
                    }

                    _log.Info($"Completed {i + 1} of {balances.Count} transfers.");
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed to transfer balance [{balance.Balance}] for client [{balance.ClientId}].");
                }
            }
        }
    }
}
