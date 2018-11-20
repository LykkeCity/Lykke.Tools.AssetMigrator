using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IBurnCommand
    {
        void Configure(
            CommandLineApplication app);
    }
}
