using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class MigrationEntity : AzureTableEntity
    {
        public Guid CashInOutId { get; set; }
    }
}