using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.SettingsReader.ReloadingManager;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    internal sealed class MigrationsRepository
    {
        private readonly INoSQLTableStorage<MigrationEntity> _migrations;
        
        
        public MigrationsRepository(
            string connectionString,
            ILogFactory logFactory,
            Guid migrationId)
        {            
            _migrations = AzureTableStorage<MigrationEntity>.Create
            (
                connectionStringManager: ConstantReloadingManager.From(connectionString),
                tableName: $"BalanceMigrationsX{migrationId:N}",
                logFactory: logFactory
            );
        }
        
        
        public async Task<Guid> GetOrCreateCashInOutIdAsync(
            string clientId,
            string assetId)
        {
            var migration = await _migrations.GetOrInsertAsync
            (
                clientId,
                assetId,
                () => new MigrationEntity
                {
                    CashInOutId = Guid.NewGuid(),
                        
                    PartitionKey = clientId,
                    RowKey = assetId
                }
             );

            return migration.CashInOutId;
        }
    }
}
