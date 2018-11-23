using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.OperationsRepository.AutorestClient.Models;
using Lykke.Service.OperationsRepository.Client.CashOperations;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class Transferer : MigratorBase, ITransferer
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly IMigrateOptions _options;
        
        public Transferer(
            ILogFactory logFactory,
            IMigrateOptions options) 
            
            : base(logFactory, options)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }
        
        public async Task RunAsync()
        {
            _log.Info("Transfer started.");

            var balances = await GetBalancesAsync();
            
            _log.Info($"Got {balances.Length} balances to transfer.");

            await TransferAsync(balances);
            
            _log.Info("transfer completed.");
        }
        
        private async Task TransferAsync(
            IReadOnlyList<BalanceEntity> balances)
        {
            var meClient = new TcpMatchingEngineClient(_options.MEEndPoint, EmptyLogFactory.Instance);
            
            var migrationsRepository = new MigrationsRepository
            (
                _options.BalancesConnectionString,
                _logFactory,
                _options.MigrationId
            );
            
            var operationsRepository = new CashOperationsRepositoryClient
            (
                _options.OperationsUrl,
                _logFactory.CreateLog(this),
                600
            );
            
            meClient.Start();
            
            for (var i = 0; i < balances.Count; i++)
            {
                var balance = balances[i];
                
                try
                {
                    var cashInId = await migrationsRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.TargetAssetId);
                    var cashOutId = await migrationsRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.SourceAssetId);
                    var cashInAmount = ((double) balance.Balance * _options.Multiplier).TruncateDecimalPlaces((int) _options.TargetAssetAccuracy);
                    var cashOutAmount = (double) balance.Balance * -1;
                    
                    var cashInResult = await meClient.CashInOutAsync
                    (
                        id: cashInId.ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.TargetAssetId,
                        amount: cashInAmount
                    );

                    if (cashInResult.Status == MeStatusCodes.Ok || cashInResult.Status == MeStatusCodes.Duplicate)
                    {
                        var operationId = cashInId.ToString();

                        if (await operationsRepository.GetAsync(balance.ClientId, operationId) == null)
                        {
                            await operationsRepository.RegisterAsync(new CashInOutOperation
                            {
                                Amount = cashInAmount,
                                AssetId = _options.TargetAssetId,
                                ClientId = balance.ClientId,
                                DateTime = DateTime.UtcNow,
                                Id = operationId,
                                TransactionId = operationId,
                                BlockChainHash = _options.MigrationMessage,

                                Type = CashOperationType.None,
                                State = TransactionStates.SettledOffchain
                            });
                        }
                    }
                    else
                    {
                        _log.Warning($"CashIn [{cashInAmount}] for client [{balance.ClientId}] completed with [{cashInResult.Status.ToString()}] status.");
                    }

                    var cashOutResult = await meClient.CashInOutAsync
                    (
                        id: cashOutId.ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.SourceAssetId,
                        amount: cashOutAmount
                    );

                    if (cashOutResult.Status == MeStatusCodes.Ok || cashOutResult.Status == MeStatusCodes.Duplicate)
                    {
                        var operationId = cashOutId.ToString();

                        if (await operationsRepository.GetAsync(balance.ClientId, operationId) == null)
                        {
                            await operationsRepository.RegisterAsync(new CashInOutOperation
                            {
                                Amount = cashOutAmount,
                                AssetId = _options.SourceAssetId,
                                ClientId = balance.ClientId,
                                DateTime = DateTime.UtcNow,
                                Id = operationId,
                                TransactionId = operationId,
                                BlockChainHash = _options.MigrationMessage,

                                Type = CashOperationType.None,
                                State = TransactionStates.SettledOffchain
                            });
                        }
                    }
                    else
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
