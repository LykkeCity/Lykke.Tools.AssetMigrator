﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
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
        private readonly ILogger<IMigrator> _logger;
        private readonly ICommandLineOptions _options;

        
        public Migrator(
            ILogger<IMigrator> logger, 
            ICommandLineOptions options)
        {
            _logger = logger;
            _options = options;
        }

        
        public async Task RunAsync()
        {
            _logger.LogInformation("Migration started");
            
            var balances = await GetBalancesAsync();
            
            _logger.LogInformation($"{balances.Length} balances found");

            await MigrateAsync(balances);
            
            _logger.LogInformation("Migration completed");
        }

        private async Task<BalanceEntity[]> GetBalancesAsync()
        {
            var balanceRepository = new BalanceRepository(_options.BalancesConncectionString);
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

            } while (continuationToken != null);

            return balances.Where(x => x.Balance > 0).ToArray();
        }

        private async Task MigrateAsync(
            IReadOnlyList<BalanceEntity> balances)
        {
            var meClient = new TcpMatchingEngineClient(_options.MEEndPoint, EmptyLogFactory.Instance);
            var migrationsRepository = new MigrationsRepository(_options.BalancesConncectionString, _options.MigrationId);
            var operationsRepository = new CashOperationsRepositoryClient(_options.OperationsUrl, EmptyLogFactory.Instance.CreateLog(this), 600);
            
            meClient.Start();
            
            for (var i = 0; i < balances.Count; i++)
            {
                var balance = balances[i];
                
                try
                {
                    var cashInId = await migrationsRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.TargetAssetId);
                    var cashOutId = await migrationsRepository.GetOrCreateCashInOutIdAsync(balance.ClientId, _options.SourceAssetId);
                    var cashinAmount = ((double) balance.Balance * _options.Multiplier).TruncateDecimalPlaces((int) _options.TargetAssetAccuracy);
                    var cashOutAmount = (double) balance.Balance * -1;
                    
                    var cashInResult = await meClient.CashInOutAsync
                    (
                        id: cashInId.ToString(),
                        clientId: balance.ClientId,
                        assetId: _options.TargetAssetId,
                        amount: cashinAmount
                    );

                    if (cashInResult.Status == MeStatusCodes.Ok || cashInResult.Status == MeStatusCodes.Duplicate)
                    {
                        var operationId = cashInId.ToString();

                        if (await operationsRepository.GetAsync(balance.ClientId, operationId) == null)
                        {
                            await operationsRepository.RegisterAsync(new CashInOutOperation
                            {
                                Amount = cashinAmount,
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
                        _logger.LogWarning($"CashIn for client [{balance.ClientId}] completed with [{cashInResult.Status.ToString()}] status");
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
                        _logger.LogWarning($"CashOut completed for client {balance.ClientId} with [{cashInResult.Status.ToString()}] status");
                    }

                    _logger.LogInformation($"Completed {i + 1} of {balances.Count} migrations");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to migrate balance for client [{balance.ClientId}]");
                }
            }
        }
    }
}
