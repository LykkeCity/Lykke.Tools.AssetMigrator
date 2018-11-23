using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Tools.AssetMigrator.Entities
{
    public class MigrationEntity : AzureTableEntity
    {
        public Guid CashInOutId { get; set; }
    }
}
