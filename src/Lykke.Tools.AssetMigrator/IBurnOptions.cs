using System.Net;


namespace Lykke.Tools.AssetMigrator
{
    public interface IBurnOptions : IOptions
    {
        uint AssetAccuracy { get; }
        
        string AssetId { get; }
        
        string BalancesConnectionString { get; }
        
        IPEndPoint MEEndPoint { get; }
    }
}
