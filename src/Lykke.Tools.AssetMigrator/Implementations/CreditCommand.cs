using Lykke.Common.Log;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CreditCommand : CommandBase<ICreditStrategy, ICreditOptions>, ICreditCommand
    {

        public CreditCommand(
            ILogFactory logFactory,
            ICreditOptions options) :

            base("credit", logFactory, options)
        {

        }

        protected override IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(LogFactory)
                .AddSingleton(Options)
                .AddMatchingEngineClient(Options.MEEndPoint)
                .AddSingleton<ICreditStrategy, CreditStrategy>()
                .AddMigrationRepository(Options.BalancesConnectionString, Options.ExecutionId, LogFactory);
        }
    }
}
