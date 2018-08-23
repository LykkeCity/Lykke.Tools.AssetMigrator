using Lykke.AzureStorage.Tables;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BalanceEntity : AzureTableEntity
    {
        public decimal Balance { get; set; }
        
        public string ClientId => PartitionKey;
    }
}