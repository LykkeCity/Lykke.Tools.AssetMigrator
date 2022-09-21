using System.Net;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Tools.AssetMigrator.Extensions
{
    public static class CommandOptionsExtensions
    {
        public static IPEndPoint GetIPEndPoint(this CommandOption option)
        {
            var hostAndPort = option.Value().Split(":");

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
