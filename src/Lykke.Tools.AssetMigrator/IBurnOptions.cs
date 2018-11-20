using System.Net;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IBurnOptions
    {
        string AssetId { get; }
        
        string ClientId { get; }
        
        IPEndPoint MEEndPoint { get; }
        
        bool ShowHelp { get; }
        
        
        void Configure(
            CommandLineApplication app);
        
        bool Validate();
    }
}
