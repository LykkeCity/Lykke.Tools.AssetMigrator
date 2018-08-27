using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Tools.AssetMigrator.Implementations;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Tools.AssetMigrator
{
    internal static class Program
    {
        
        private static async Task<int> Main(string[] args)
        {
            int resultCode;
            ServiceProvider serviceProvider = null;

            try
            {
                serviceProvider = ConfigureServices().BuildServiceProvider();
                
                var rootCommand = serviceProvider.GetService<IRootCommand>();
                var app = rootCommand.Configure();

                resultCode = app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                resultCode = 1;
            }
            
            serviceProvider?.Dispose();

            return resultCode;
        }

        private static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton
                (
                    x => LogFactory
                        .Create()
                        .AddConsole()
                );

            serviceCollection
                .AddSingleton<ICommandLineOptions, CommandLineOptions>();
            
            serviceCollection
                .AddSingleton<IMigrator, Migrator>();

            serviceCollection
                .AddSingleton<IRootCommand, RootCommand>();
            
            return serviceCollection;
        }
    }
}
