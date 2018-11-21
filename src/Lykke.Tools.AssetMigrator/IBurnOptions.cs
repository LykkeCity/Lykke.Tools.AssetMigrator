using System.Net;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IBurnOptions
    {
        uint AssetAccuracy { get; }
        
        string AssetId { get; }
        
        string BalancesConnectionString { get; }
        
        string ClientId { get; }
        
        IPEndPoint MEEndPoint { get; }
        
        bool ShowHelp { get; }
        
        
        void Configure(
            CommandLineApplication app);
        
        bool Validate();
    }
}
