using System;
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
            var operationsRepository = new CashOperationsRepositoryClient(_options.OperationsUrl, EmptyLogFactory.Instance.CreateLog(this), 60);
            
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
                        await operationsRepository.RegisterAsync(new CashInOutOperation
                        {
                            Amount = cashinAmount,
                            AssetId = _options.TargetAssetId,
                            ClientId = balance.ClientId,
                            DateTime = DateTime.UtcNow,
                            Id = cashInId.ToString(),

                            Type = CashOperationType.None,
                            State = TransactionStates.SettledOffchain
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"CashIn completed with [{cashInResult.Status.ToString()}] status");
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
                        await operationsRepository.RegisterAsync(new CashInOutOperation
                        {
                            Amount = cashOutAmount,
                            AssetId = _options.SourceAssetId,
                            ClientId = balance.ClientId,
                            DateTime = DateTime.UtcNow,
                            Id = cashOutId.ToString(),

                            Type = CashOperationType.None,
                            State = TransactionStates.SettledOffchain
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"CashOut cimpleted with [{cashInResult.Status.ToString()}] status");
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
