using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.MatchingEngine.Connector.Services;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class Burner : IBurner
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly IBurnOptions _options;

        public Burner(
            IBurnOptions options,
            ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }

        
        public async Task RunAsync()
        {
            _log.Info("Balance burning started");
            
            var meClient = new TcpMatchingEngineClient(_options.MEEndPoint, _logFactory);

            meClient.Start();
            
            await meClient.UpdateBalanceAsync
            (
                id: Guid.NewGuid().ToString(),
                clientId: _options.ClientId,
                assetId: _options.AssetId,
                value: 0
            );
            
            _log.Info("Balance burning completed");
        }
    }
}
