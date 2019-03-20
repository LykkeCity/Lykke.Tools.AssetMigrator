using Lykke.Common.Log;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class TransferCommand : CommandBase<ITransferStrategy, ITransferOptions>, ITransferCommand
    {
        public TransferCommand(
            ILogFactory logFactory,
            ITransferOptions options) 
            
            : base("transfer", logFactory, options)
        {
            
        }
        
        protected override IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(LogFactory)
                .AddSingleton(Options)
                .AddBalanceRepository(Options.BalancesConnectionString, LogFactory)
                .AddMatchingEngineClient(Options.MEEndPoint)
                .AddMigrationRepository(Options.BalancesConnectionString, Options.MigrationId, LogFactory)
                .AddSingleton<IBalanceService, BalanceService>()
                .AddSingleton<ITransferStrategy, TransferStrategy>();
        }
    }
}
