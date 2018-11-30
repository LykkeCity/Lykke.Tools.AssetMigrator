using Lykke.Common.Log;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CopyCommand : CommandBase<ICopyStrategy, IMigrateOptions>, ICopyCommand
    {
        public CopyCommand(
            ILogFactory logFactory,
            IMigrateOptions options) : 
            
            base("copy", logFactory, options)
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
                .AddSingleton<ICopyStrategy, CopyStrategy>();
        }
    }
}
