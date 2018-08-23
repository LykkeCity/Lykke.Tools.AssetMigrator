using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator
{
    public interface IRootCommand
    {
        CommandLineApplication Configure();
    }
}