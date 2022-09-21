using System;
using System.Net;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnOptions : IBurnOptions
    {
        private bool _optionsConfigured;

        private CommandOption _assetAccuracy;
        private CommandOption _assetId;
        private CommandOption _balancesConnectionString;
        private CommandOption _help;
        private CommandOption _meEndPoint;

        public uint AssetAccuracy
            => uint.Parse(_assetAccuracy.Value());
        
        public string AssetId
            => _assetId.Value();

        public string BalancesConnectionString
            => _balancesConnectionString.Value();

        public IPEndPoint MEEndPoint
            => _meEndPoint.GetIPEndPoint();
        
        public bool ShowHelp
            => _help.HasValue();

        
        public void Configure(
            CommandLineApplication app)
        {
            _assetAccuracy = app.Option
            (
                "--asset-accuracy",
                "Asset accuracy",
                CommandOptionType.SingleValue
            );
            
            _assetId = app.Option
            (
                "--asset-id",
                "Asset id",
                CommandOptionType.SingleValue
            );
            
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

            _optionsConfigured = true;
        }

        public bool Validate()
        {
            if (!_optionsConfigured)
            {
                throw new Exception("Options have not been configured.");
            }

            var optionsAreValid = true;
            
            if (!_assetAccuracy.HasValue())
            {
                Console.WriteLine("Asset accuracy is not provided");
                
                optionsAreValid = false;
            }
            else if (!uint.TryParse(_assetAccuracy.Value(), out _))
            {
                Console.WriteLine("Asset accuracy should be greater or equal to zero");
                
                optionsAreValid = false;
            }
            
            if (!_assetId.HasValue())
            {
                Console.WriteLine("Asset id is not provided");
                
                optionsAreValid = false;
            }
            
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

            return optionsAreValid;
        }
    }
}
