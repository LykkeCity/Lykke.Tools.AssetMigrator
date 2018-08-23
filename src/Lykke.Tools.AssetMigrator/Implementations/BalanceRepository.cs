using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Logs;
using Microsoft.WindowsAzure.Storage.Table;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    internal sealed class BalanceRepository
    {
        private readonly INoSQLTableStorage<BalanceEntity> _balances;

        public BalanceRepository(
            string connectionString)
        {
            _balances = AzureTableStorage<BalanceEntity>.Create
            (
                connectionStringManager: new ConnectionStringManager(connectionString),
                tableName: "Balances",
                logFactory: EmptyLogFactory.Instance
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