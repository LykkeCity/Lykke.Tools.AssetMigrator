using System;
using System.Net;


namespace Lykke.Tools.AssetMigrator
{
    public interface IMigrateOptions : IOptions
    {
        string BalancesConnectionString { get; }
        
        IPEndPoint MEEndPoint { get; }
        
        Guid MigrationId { get; }
        
        uint Multiplier { get; }

        string SourceAssetId { get; }

        uint TargetAssetAccuracy { get; }
        
        string TargetAssetId { get; }
    }
}
