using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class TransferCommand : ITransferCommand
    {
        private readonly ILog _log;
        private readonly IMigrateOptions _options;
        private readonly ITransferer _transferer;

        
        public TransferCommand(
            ILogFactory logFactory,
            IMigrateOptions options,
            ITransferer transferer)
        {
            _log = logFactory.CreateLog(this);
            _options = options;
            _transferer = transferer;
        }

        
        public void Configure(
            CommandLineApplication app)
        {
            app.Command("transfer", cmd =>
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
                    await _transferer.RunAsync();
                }
                
                return 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Transfer failed.");
                
                return 1;
            }
        }
    }
}
