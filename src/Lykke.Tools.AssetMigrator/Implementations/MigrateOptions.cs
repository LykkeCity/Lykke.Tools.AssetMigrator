using System;
using System.Net;
using JetBrains.Annotations;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.CommandLineUtils;


namespace Lykke.Tools.AssetMigrator.Implementations
{
    [UsedImplicitly]
    public abstract class MigrateOptions : IMigrateOptions
    {
        private bool _optionsConfigured;
        
        private CommandOption _balancesConnectionString;
        private CommandOption _help;
        private CommandOption _meEndPoint;
        private CommandOption _migrationId;
        private CommandOption _multiplier;
        private CommandOption _operationsUrl;
        private CommandOption _sourceAssetId;
        private CommandOption _targetAssetAccuracy;
        private CommandOption _targetAssetId;


        public string BalancesConnectionString
            => _balancesConnectionString.Value();

        public bool ShowHelp
            => _help.HasValue();

        public IPEndPoint MEEndPoint
            => _meEndPoint.GetIPEndPoint();

        public Guid MigrationId
            => _migrationId.HasValue() ? Guid.Parse(_migrationId.Value()) : Guid.Empty;
        
        public uint Multiplier
            => _multiplier.HasValue() ? uint.Parse(_multiplier.Value()) : 1;

        public string OperationsUrl
            => _operationsUrl.Value();

        public string SourceAssetId
            => _sourceAssetId.Value();

        public uint TargetAssetAccuracy
            => uint.Parse(_targetAssetAccuracy.Value());
        
        public string TargetAssetId
            => _targetAssetId.Value();
        
        
        public void Configure(
            CommandLineApplication app)
        {
            _balancesConnectionString = app.Option
            (
                "--balances-conn-string",
                "Lykke.Service.Balances connection string",
                CommandOptionType.SingleValue
            );
            
            _help = app.HelpOption
            (
                "-h|--help"
            );

            _meEndPoint = app.Option
            (
                "--me-endpoint",
                "ME endpoint (host:port)",
                CommandOptionType.SingleValue
            );
            
            _operationsUrl = app.Option
            (
                "--operations-url",
                "Lykke.Service.OperationsRepository url",
                CommandOptionType.SingleValue
            );
            
            _sourceAssetId = app.Option
            (
                "--from",
                "Source asset id",
                CommandOptionType.SingleValue
            );
            
            _targetAssetAccuracy = app.Option
            (
                "--accuracy",
                "Target asset accuracy",
                CommandOptionType.SingleValue
            );
            
            _targetAssetId = app.Option
            (
                "--to",
                "Target asset id",
                CommandOptionType.SingleValue
            );
            
            _migrationId = app.Option
            (
                "--migration-id",
                "Migration id (guid, optional)",
                CommandOptionType.SingleValue
            );

            _multiplier = app.Option
            (
                "--multiplier",
                "Multiplier (greater or equal to one, optional)",
                CommandOptionType.SingleValue
            );

            _optionsConfigured = true;
        }

        public bool Validate()
        {
            if (!_optionsConfigured)
            {
                throw new Exception("Options have not been configured.");
            }

            var optionsAreValid = true;
            
            if (!_balancesConnectionString.HasValue())
            {
                Console.WriteLine("Balances connection string is not provided");
                
                optionsAreValid = false;
            }

            if (!_meEndPoint.HasValue())
            {
                Console.WriteLine("ME endpoint is not provided");
                
                optionsAreValid = false;
            }

            if (_migrationId.HasValue() && !Guid.TryParse(_migrationId.Value(), out _))
            {
                Console.WriteLine("Migration id should be a valid guid");
                
                optionsAreValid = false;
            }
            
            if (_multiplier.HasValue() && (!uint.TryParse(_multiplier.Value(), out var multiplier) || multiplier == 0))
            {
                Console.WriteLine("Multiplier should be greater or equal to one");
                
                optionsAreValid = false;
            }

            if (!_operationsUrl.HasValue())
            {
                Console.WriteLine("Lykke.Service.OperationsRepository url is not provided");
                
                optionsAreValid = false;
            }
            else if (!Uri.TryCreate(_operationsUrl.Value(), UriKind.Absolute, out _))
            {
                Console.WriteLine("Lykke.Service.OperationsRepository url is invalid");
                
                optionsAreValid = false;
            }
            
            if (!_sourceAssetId.HasValue())
            {
                Console.WriteLine("Source asset id is not provided");
                
                optionsAreValid = false;
            }

            if (!_targetAssetAccuracy.HasValue())
            {
                Console.WriteLine("Target asset accuracy is not provided");
                
                optionsAreValid = false;
            }
            else if (!uint.TryParse(_targetAssetAccuracy.Value(), out _))
            {
                Console.WriteLine("Target asset accuracy should be greater or equal to zero");
                
                optionsAreValid = false;
            }
            
            if (!_targetAssetId.HasValue())
            {
                Console.WriteLine("Target asset id is not provided");
                
                optionsAreValid = false;
            }
            
            if (!optionsAreValid)
            {
                Console.WriteLine();
            }
            
            return optionsAreValid;
        }
    }
}
