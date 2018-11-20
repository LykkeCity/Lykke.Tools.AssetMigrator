using System.Threading.Tasks;

namespace Lykke.Tools.AssetMigrator
{
    public interface ICopier
    {
        Task RunAsync();
    }
}
