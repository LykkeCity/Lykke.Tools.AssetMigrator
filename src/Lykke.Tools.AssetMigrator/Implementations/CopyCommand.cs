using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CopyCommand : ICopyCommand
    {
        private readonly ICopier _copier;
        private readonly ILog _log;
        private readonly IMigrateOptions _options;

        
        public CopyCommand(
            ICopier copier,
            ILogFactory logFactory,
            IMigrateOptions options)
        {
            _copier = copier;
            _log = logFactory.CreateLog(this);
            _options = options;
        }

        
        public void Configure(
            CommandLineApplication app)
        {
            app.Command("copy", cmd =>
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
                    await _copier.RunAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Copying failed.");
                
                return 1;
            }
        }
    }
}
