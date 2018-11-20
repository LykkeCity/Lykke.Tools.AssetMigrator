using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface ITransferCommand
    {
        void Configure(
            CommandLineApplication app);
    }
}
