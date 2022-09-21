using System;
using System.Net;


namespace Lykke.Tools.AssetMigrator
{
    public interface ICreditOptions : IOptions
    {
        IPEndPoint MEEndPoint { get; }

        string CreditPlanCsvPath { get; }

        Guid ExecutionId { get; }

        string BalancesConnectionString { get; }
    }
}
