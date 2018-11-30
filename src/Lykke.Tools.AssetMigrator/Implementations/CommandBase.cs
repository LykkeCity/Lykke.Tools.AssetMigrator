using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    public abstract class CommandBase<TStrategy, TOptions> : IChildCommand
        where TStrategy : IStrategy
        where TOptions : IOptions
    {
        private readonly string _commandName;
        private readonly ILog _log;


        protected CommandBase(
            string commandName,
            ILogFactory logFactory,
            TOptions options)
        {
            _commandName = commandName;
            _log = logFactory.CreateLog(this);

            LogFactory = logFactory;
            Options = options;
        }
        
        
        protected ILogFactory LogFactory { get; }
        
        protected TOptions Options { get; }
        
        
        public void Configure(
            CommandLineApplication app)
        {
            app.Command(_commandName, cmd =>
            {
                Options.Configure(cmd);
                
                cmd.OnExecute(() => ExecuteAsync(cmd));
            });
        }

        private async Task<int> ExecuteAsync(
            CommandLineApplication cmd)
        {
            int resultCode;
            ServiceProvider serviceProvider = null;
            
            try
            {
                if (Options.ShowHelp || !Options.Validate())
                {
                    cmd.ShowHelp();
                }
                else
                {
                    serviceProvider = ConfigureServices().BuildServiceProvider();
                    
                    await serviceProvider
                        .GetService<TStrategy>()
                        .ExecuteAsync();
                }
                
                resultCode = 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Execution burning failed.");
                
                resultCode = 1;
            }

            serviceProvider?.Dispose();
            
            return resultCode;
        }
        
        protected abstract IServiceCollection ConfigureServices();
    }
}
