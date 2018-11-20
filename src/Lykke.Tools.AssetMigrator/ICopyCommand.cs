using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface ICopyCommand
    {
        void Configure(
            CommandLineApplication app);
    }
}
