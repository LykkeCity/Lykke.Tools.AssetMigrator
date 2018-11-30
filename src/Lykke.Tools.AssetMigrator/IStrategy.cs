using System.Threading.Tasks;

namespace Lykke.Tools.AssetMigrator
{
    public interface IStrategy
    {
        Task ExecuteAsync();
    }
}
