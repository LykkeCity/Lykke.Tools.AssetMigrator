using System;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public sealed class RootCommand : IRootCommand
    {
        private readonly CommandLineApplication _app;
        private readonly ILog _log;
        private readonly IMigrator _migrator;
        private readonly ICommandLineOptions _options;

        
        public RootCommand(
            ILogFactory logFactory,
            IMigrator migrator,
            ICommandLineOptions options)
        {
            _app = new CommandLineApplication(throwOnUnexpectedArg: false);
            _log = logFactory.CreateLog(this);
            _migrator = migrator;
            _options = options;
        }


        public CommandLineApplication Configure()
        {
            _options.Configure(_app);

            _app.OnExecute(() => ExecuteAsync());
            
            _app.VersionOption("--version", GetVersion);
            
            return _app;
        }

        private async Task<int> ExecuteAsync()
        {
            try
            {
                if (_options.ShowHelp || !_options.Validate())
                {
                    _app.ShowHelp();
                }
                else
                {
                    await _migrator.RunAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Migration failed.");

                return 1;
            }
        }

        private static string GetVersion()
        {
            return typeof(RootCommand).GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;            
        }
    }
}
