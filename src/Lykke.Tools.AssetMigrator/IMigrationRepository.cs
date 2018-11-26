using System;
using System.Threading.Tasks;

namespace Lykke.Tools.AssetMigrator
{
    public interface IMigrationRepository
    {
        Task<Guid> GetOrCreateCashInOutIdAsync(
            string clientId,
            string assetId);
    }
}
