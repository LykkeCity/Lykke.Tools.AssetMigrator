using System;
using System.Net;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class BurnOptions : IBurnOptions
    {
        private bool _optionsConfigured;

        private CommandOption _assetAccuracy;
        private CommandOption _assetId;
        private CommandOption _balancesConnectionString;
        private CommandOption _clientId;
        private CommandOption _help;
        private CommandOption _meEndPoint;

        public uint AssetAccuracy
            => uint.Parse(_assetAccuracy.Value());
        
        public string AssetId
            => _assetId.Value();

        public string BalancesConnectionString
            => _balancesConnectionString.Value();
        
        public string ClientId
            => _clientId.Value();
        
        public IPEndPoint MEEndPoint
            => GetMEEndPoint();
        
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
            
            _clientId = app.Option
            (
                "--client-id",
                "Client id",
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
            
            if (!_clientId.HasValue())
            {
                Console.WriteLine("Client id is not provided");
                
                optionsAreValid = false;
            }
            
            if (!_meEndPoint.HasValue())
            {
                Console.WriteLine("ME endpoint is not provided");
                
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
        
        private IPEndPoint GetMEEndPoint()
        {
            var hostAndPort = _meEndPoint.Value().Split(":");
            
            if (IPAddress.TryParse(hostAndPort[0], out var ipAddress))
            {
                return new IPEndPoint(ipAddress, int.Parse(hostAndPort[1]));
            }
            else
            {
                var addresses = Dns.GetHostAddressesAsync(hostAndPort[0]).Result;
            
                return new IPEndPoint(addresses[0], int.Parse(hostAndPort[1]));
            }
        }
    }
}
