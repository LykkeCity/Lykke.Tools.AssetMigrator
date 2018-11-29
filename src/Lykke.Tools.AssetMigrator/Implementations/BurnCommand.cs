using Lykke.Common.Log;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnCommand : CommandBase<IBurnStrategy, IBurnOptions>, IBurnCommand
    {
        public BurnCommand(
            ILogFactory logFactory,
            IBurnOptions options) : 
            
            base("burn", logFactory, options)
        {
            
        }
        
        protected override IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(LogFactory)
                .AddSingleton(Options)
                .AddBalanceRepository(Options.BalancesConnectionString, LogFactory)
                .AddMatchingEngineClient(Options.MEEndPoint)
                .AddSingleton<IBalanceService, BalanceService>()
                .AddSingleton<IBurnStrategy, BurnStrategy>();
        }
    }
}
