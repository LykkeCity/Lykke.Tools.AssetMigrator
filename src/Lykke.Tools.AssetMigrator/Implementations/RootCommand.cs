using System;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public sealed class RootCommand : IRootCommand
    {
        private readonly CommandLineApplication _app;
        private readonly ILogger<IRootCommand> _log;
        private readonly IMigrator _migrator;
        private readonly ICommandLineOptions _options;

        
        public RootCommand(
            ILogger<IRootCommand> log,
            IMigrator migrator,
            ICommandLineOptions options)
        {
            _app = new CommandLineApplication(throwOnUnexpectedArg: false);
            _log = log;
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
                if (_options.ShowHelp || !await _options.ValidateAsync())
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
                _log.LogCritical(e, "Migration failed.");

                await Task.Delay(1000);
                
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