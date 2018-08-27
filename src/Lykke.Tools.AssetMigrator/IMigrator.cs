using System.Threading.Tasks;

namespace Lykke.Tools.AssetMigrator
{
    public interface IMigrator
    {
        Task RunAsync();
    }
}