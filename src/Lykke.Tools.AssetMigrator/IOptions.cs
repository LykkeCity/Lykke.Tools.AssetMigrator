using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IOptions
    {
        bool ShowHelp { get; }
        
        void Configure(
            CommandLineApplication app);
        
        bool Validate();
    }
}
