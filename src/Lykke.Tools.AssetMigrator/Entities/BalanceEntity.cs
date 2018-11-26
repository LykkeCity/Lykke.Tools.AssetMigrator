using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Tools.AssetMigrator.Entities
{
    public class BalanceEntity : AzureTableEntity
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public decimal Balance { get; set; }
        
        public string ClientId => PartitionKey;
    }
}
