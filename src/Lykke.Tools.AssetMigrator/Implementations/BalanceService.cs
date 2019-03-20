using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Tools.AssetMigrator.Entities;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly string _assetId;
        private readonly IBalanceRepository _balanceRepository;
        private readonly ILog _log;


        public BalanceService(
            IBalanceRepository balanceRepository,
            ILogFactory logFactory,
            IBurnOptions options)
        {
            _assetId = options.AssetId;
            _balanceRepository = balanceRepository;
            _log = logFactory.CreateLog(this);
        }
        
        private BalanceService(
            IBalanceRepository balanceRepository,
            ILogFactory logFactory,
            IMigrateOptions options)
        {
            _assetId = options.SourceAssetId;
            _balanceRepository = balanceRepository;
            _log = logFactory.CreateLog(this);
        }
        
        public BalanceService(
            IBalanceRepository balanceRepository,
            ILogFactory logFactory,
            ICopyOptions options)
            
            : this(balanceRepository, logFactory, (IMigrateOptions) options)
        {
            
        }
        
        public BalanceService(
            IBalanceRepository balanceRepository,
            ILogFactory logFactory,
            ITransferOptions options)
        
            : this(balanceRepository, logFactory, (IMigrateOptions) options)
        {
            
        }

        public async Task<BalanceEntity[]> GetBalancesAsync()
        {
            var balances = new List<BalanceEntity>();

            string continuationToken = null;
            
            do
            {
                IEnumerable<BalanceEntity> balancesBatch;
                
                (balancesBatch, continuationToken) = await _balanceRepository.GetBalancesAsync
                (
                    _assetId,
                    100,
                    continuationToken
                );
                
                balances.AddRange(balancesBatch.Where(x => x.ClientId != "TotalBalance"));
                
                _log.Info($"{balances.Count} balances found.");

            } while (continuationToken != null);

            return balances.Where(x => x.Balance > 0).ToArray();
        }
    }
}
