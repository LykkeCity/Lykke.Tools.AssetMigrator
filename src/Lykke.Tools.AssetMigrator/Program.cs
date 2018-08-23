using System;
using Lykke.Tools.AssetMigrator.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Lykke.Tools.AssetMigrator
{
    internal static class Program
    {
        
        private static int Main(string[] args)
        {
            try
            {
                var services = ConfigureServices();
                var serviceProvider = services.BuildServiceProvider();
                var rootCommand = serviceProvider.GetService<IRootCommand>();
                var app = rootCommand.Configure();

                return app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return 1;
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging
            (
                options =>
                {
                    options.AddConsole();
                }
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