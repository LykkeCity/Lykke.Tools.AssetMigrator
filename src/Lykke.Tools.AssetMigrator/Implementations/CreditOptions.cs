using System;
using System.Net;
using Lykke.Tools.AssetMigrator.Extensions;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class CreditOptions : ICreditOptions
    {
        private bool _optionsConfigured;

        private CommandOption _creditPlanCsvPath;
        private CommandOption _help;
        private CommandOption _meEndPoint;
        private CommandOption _executionId;
        private CommandOption _balancesConnectionString;

        public IPEndPoint MEEndPoint 
            => _meEndPoint.GetIPEndPoint();

        public string CreditPlanCsvPath
            => _creditPlanCsvPath.Value();

        public bool ShowHelp 
            => _help.HasValue();

        public Guid ExecutionId
            => Guid.Parse(_executionId.Value());

        public string BalancesConnectionString
            => _balancesConnectionString.Value();

        public void Configure(CommandLineApplication app)
        {
            _help = app.HelpOption
            (
                "-h|--help"
            );

            _creditPlanCsvPath = app.Option
            (
                "--credit-plan-csv-path",
                "Path to the CSV file with the credit plan in format: [wallet_id, asset_id, asset_accuracy, amount] and header",
                CommandOptionType.SingleValue
            );            

            _meEndPoint = app.Option
            (
                "--me-endpoint",
                "ME endpoint (host:port)",
                CommandOptionType.SingleValue
            );

            _executionId = app.Option
            (
                "--execution-id",
                "Unique execution id (guid)",
                CommandOptionType.SingleValue
            );

            _balancesConnectionString = app.Option
            (
                "--balances-conn-string",
                "Lykke.Service.Balances connection string",
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

            if (!_creditPlanCsvPath.HasValue())
            {
                Console.WriteLine("Credit plan CSV path is not provided");

                optionsAreValid = false;
            }

            if (!_meEndPoint.HasValue())
            {
                Console.WriteLine("ME endpoint is not provided");

                optionsAreValid = false;
            }

            if (!_balancesConnectionString.HasValue())
            {
                Console.WriteLine("Balances connection string is not provided");

                optionsAreValid = false;
            }

            if (!_executionId.HasValue() || !Guid.TryParse(_executionId.Value(), out _))
            {
                Console.WriteLine("Execution id should be a valid guid");

                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }
}
