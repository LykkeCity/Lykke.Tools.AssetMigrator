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


namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public abstract class MigratorBase
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly IMigrateOptions _options;

        
        protected MigratorBase(
            ILogFactory logFactory,
            IMigrateOptions options)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }

        protected async Task<BalanceEntity[]> GetBalancesAsync()
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
                
                balances.AddRange(balancesBatch.Where(x => x.ClientId != "TotalBalance"));
                
                _log.Info($"{balances.Count} balances found.");

            } while (continuationToken != null);

            return balances.Where(x => x.Balance > 0).ToArray();
        }
    }
}
