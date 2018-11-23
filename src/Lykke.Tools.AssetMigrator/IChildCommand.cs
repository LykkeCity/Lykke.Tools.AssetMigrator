using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IChildCommand
    {
        void Configure(
            CommandLineApplication app);
    }
}
