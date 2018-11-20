using System.Threading.Tasks;

namespace Lykke.Tools.AssetMigrator
{
    public interface ITransferer
    {
        Task RunAsync();
    }
}
