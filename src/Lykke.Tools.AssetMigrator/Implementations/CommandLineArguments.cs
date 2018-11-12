using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CommandLineArguments : ICommandLineArguments
    {
        private CommandArgument _mode;

        
        public MigrationMode Mode 
            => Enum.Parse<MigrationMode>(_mode.Value);


        public void Configure(
            CommandLineApplication app)
        {
            _mode = app.Argument
            (
                "mode",
                "Migration mode, either 'Transfer' or 'Copy'"
            );
        }

        public bool Validate()
        {
            return Enum.TryParse<MigrationMode>(_mode.Value, out _);
        }
    }
}
