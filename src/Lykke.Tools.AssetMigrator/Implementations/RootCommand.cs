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
        private readonly IBurnCommand _burnCommand;
        private readonly ICopyCommand _copyCommand;
        private readonly ILog _log;
        private readonly ITransferCommand _transferCommand;

        
        public RootCommand(
            IBurnCommand burnCommand,
            ICopyCommand copyCommand,
            ILogFactory logFactory,
            ITransferCommand transferCommand)
        {
            _app = new CommandLineApplication(throwOnUnexpectedArg: false);
            _burnCommand = burnCommand;
            _copyCommand = copyCommand;
            _log = logFactory.CreateLog(this);
            _transferCommand = transferCommand;
        }


        public CommandLineApplication Configure()
        {
            _burnCommand.Configure(_app);
            _copyCommand.Configure(_app);
            _transferCommand.Configure(_app);
            
            _app.OnExecute(() => ExecuteAsync());
            
            _app.VersionOption("--version", GetVersion);
            
            return _app;
        }

        private Task<int> ExecuteAsync()
        {
            try
            {
                _app.ShowHelp();

                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                _log.Critical(e, "Execution failed.");
                
                return Task.FromResult(1);
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
