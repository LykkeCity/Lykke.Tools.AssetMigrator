using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.OperationsRepository.AutorestClient.Models;
using Lykke.Service.OperationsRepository.Client.CashOperations;
using Microsoft.Extensions.Logging;



namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public sealed class Migrator : IMigrator
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly ICommandLineOptions _options;

        
        public Migrator(
            ILogFactory logFactory,
            ICommandLineOptions options)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }

        
        public async Task RunAsync()
        {
            _log.Info("Migration started");
            
            var balances = await GetBalancesAsync();
            
            _log.Info($"{balances.Length} balances found");

            await MigrateAsync(balances);
            
            _log.Info("Migration completed");
        }

        private async Task<BalanceEntity[]> GetBalancesAsync()
        {
            var balanceRepository = new BalanceRepository
            (
                _options.BalancesConnectionString,
                _logFactory
            );
            
            var balances = new List<BalanceEntity>();

            string continuationToken = null;
            
            do
            {
                IEnumerable<BalanceEntity> balancesBatch;
                
                (balancesBatch, continuationToken) = await balanceRepository.GetBalancesAsync
                (
                    _options.SourceAssetId,
                    100,
                    continuationToken
                );
                
                balances.AddRange(balancesBatch);
                
                _log.Info($"Got {balances.Count} balances to migrate");

            } while (continuationToken != null);

            return balances.Where(x => x.Balance > 0).ToArray();
        }

        private async Task MigrateAsync(
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
                                BlockChainHash = "0x",

                                Type = CashOperationType.None,
                                State = TransactionStates.SettledOffchain
                            });
                        }
                    }
                    else
                    {
                        _log.Warning($"CashIn for client [{balance.ClientId}] completed with [{cashInResult.Status.ToString()}] status");
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
                                BlockChainHash = "0x",

                                Type = CashOperationType.None,
                                State = TransactionStates.SettledOffchain
                            });
                        }
                    }
                    else
                    {
                        _log.Warning($"CashOut completed for client {balance.ClientId} with [{cashInResult.Status.ToString()}] status");
                    }

                    _log.Info($"Completed {i + 1} of {balances.Count} migrations");
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Failed to migrate balance for client [{balance.ClientId}]");
                }
            }
        }
    }
}
