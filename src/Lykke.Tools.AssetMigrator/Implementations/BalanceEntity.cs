using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BalanceEntity : AzureTableEntity
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public decimal Balance { get; set; }
        
        public string ClientId => PartitionKey;
    }
}
