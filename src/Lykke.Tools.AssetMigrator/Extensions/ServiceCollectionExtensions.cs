using System;
using System.Net;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Tools.AssetMigrator.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Tools.AssetMigrator.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMatchingEngineClient(
            this IServiceCollection services,
            IPEndPoint endpoint)
        {
            return services
                .AddSingleton<IMatchingEngineClient>(x =>
                {
                    var meClient = new TcpMatchingEngineClient(endpoint, EmptyLogFactory.Instance);

                    meClient.Start();

                    return meClient; 
                });
        }

        public static IServiceCollection AddBalanceRepository(
            this IServiceCollection services,
            string connectionString,
            ILogFactory logFactory)
        {
            return services
                .AddSingleton<IBalanceRepository>(new BalanceRepository
                (
                    connectionString,
                    logFactory
                ));
        }
        
        public static IServiceCollection AddMigrationRepository(
            this IServiceCollection services,
            string connectionString,
            Guid migrationId,
            ILogFactory logFactory)
        {
            return services
                .AddSingleton<IMigrationRepository>(new MigrationRepository
                (
                    connectionString,
                    logFactory,
                    migrationId
                ));
        }
    }
}
