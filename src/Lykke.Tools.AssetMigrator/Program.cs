using System;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Tools.AssetMigrator.Implementations;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Tools.AssetMigrator
{
    internal static class Program
    {
        
        private static int Main(string[] args)
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
                .AddSingleton<IBurnOptions, BurnOptions>();
            
            serviceCollection
                .AddSingleton<IMigrateOptions, MigrateOptions>();
            
            serviceCollection
                .AddSingleton<IRootCommand, RootCommand>();
            
            serviceCollection
                .AddSingleton<IBurnCommand, BurnCommand>();

            serviceCollection
                .AddSingleton<ICopyCommand, CopyCommand>();
            
            serviceCollection
                .AddSingleton<ITransferCommand, TransferCommand>();
            
            return serviceCollection;
        }
    }
}
