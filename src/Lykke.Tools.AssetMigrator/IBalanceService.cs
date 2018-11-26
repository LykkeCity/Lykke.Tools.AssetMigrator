using System.Threading.Tasks;
using Lykke.Tools.AssetMigrator.Entities;

namespace Lykke.Tools.AssetMigrator
{
    public interface IBalanceService
    {
        Task<BalanceEntity[]> GetBalancesAsync();
    }
}
