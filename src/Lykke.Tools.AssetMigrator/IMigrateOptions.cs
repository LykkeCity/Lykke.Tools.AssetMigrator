using System;
using System.Net;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IMigrateOptions
    {
        string BalancesConnectionString { get; }
        
        IPEndPoint MEEndPoint { get; }
        
        Guid MigrationId { get; }
        
        string MigrationMessage { get; }
        
        uint Multiplier { get; }

        string OperationsUrl { get; }
        
        bool ShowHelp { get; }
        
        string SourceAssetId { get; }

        uint TargetAssetAccuracy { get; }
        
        string TargetAssetId { get; }


        void Configure(
            CommandLineApplication app);

        bool Validate();
    }
}
