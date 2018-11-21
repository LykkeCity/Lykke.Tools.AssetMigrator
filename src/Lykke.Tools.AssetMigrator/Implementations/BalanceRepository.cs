using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.WindowsAzure.Storage.Table;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    internal sealed class BalanceRepository
    {
        private readonly INoSQLTableStorage<BalanceEntity> _balances;

        public BalanceRepository(
            string connectionString,
            ILogFactory logFactory)
        {
            _balances = AzureTableStorage<BalanceEntity>.Create
            (
                connectionStringManager: ConstantReloadingManager.From(connectionString),
                tableName: "Balances",
                logFactory: logFactory
            );
        }


        public Task<BalanceEntity> TryGetBalanceAsync(
            string clientId,
            string assetId)
        {
            return _balances.GetDataAsync
            (
                partition: clientId,
                row: assetId
            );
        }
        
        public Task<(IEnumerable<BalanceEntity> balances, string continuationToken)> GetBalancesAsync(
            string assetId,
            int take,
            string continuationToken)
        {
            var filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, assetId);
            var query = new TableQuery<BalanceEntity>().Where(filter);
            
            return _balances.GetDataWithContinuationTokenAsync(query, take, continuationToken);
        }
    }
}
