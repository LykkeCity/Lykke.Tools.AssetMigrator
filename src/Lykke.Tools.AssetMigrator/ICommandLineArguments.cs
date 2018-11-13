using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface ICommandLineArguments
    {
        MigrationMode Mode { get; }


        void Configure(
            CommandLineApplication app);
        
        bool Validate();
    }
}
