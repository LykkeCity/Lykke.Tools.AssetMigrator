using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Tools.AssetMigrator.Entities;

namespace Lykke.Tools.AssetMigrator
{
    public interface IBalanceRepository
    {
        Task<BalanceEntity> TryGetBalanceAsync(
            string clientId,
            string assetId);

        Task<(IEnumerable<BalanceEntity> balances, string continuationToken)> GetBalancesAsync(
            string assetId,
            int take,
            string continuationToken);
    }
}
