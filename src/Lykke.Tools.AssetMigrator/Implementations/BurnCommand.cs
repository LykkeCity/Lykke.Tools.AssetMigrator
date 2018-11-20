using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnCommand : IBurnCommand
    {
        private readonly IBurner _burner;
        private readonly IBurnOptions _options;
        private readonly ILog _log;
        
        
        public BurnCommand(
            IBurnOptions options,
            IBurner burner,
            ILogFactory logFactory)
        {
            _options = options;
            _burner = burner;
            _log = logFactory.CreateLog(this);
        }
        
        
        public void Configure(
            CommandLineApplication app)
        {
            app.Command("burn-balance", cmd =>
            {
                _options.Configure(cmd);
                
                cmd.OnExecute(() => ExecuteAsync(cmd));
            });
        }
        
        private async Task<int> ExecuteAsync(
            CommandLineApplication cmd)
        {
            try
            {
                if (_options.ShowHelp || !_options.Validate())
                {
                    cmd.ShowHelp();
                }
                else
                {
                    await _burner.RunAsync();
                }
                
                return 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Balance burning failed.");
                
                return 1;
            }
        }
    }
}
