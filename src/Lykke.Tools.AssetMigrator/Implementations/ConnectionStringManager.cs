using System.Threading.Tasks;
using Lykke.SettingsReader;

namespace Lykke.Tools.AssetMigrator.Implementations
{
    public class ConnectionStringManager : IReloadingManager<string>
    {
        public ConnectionStringManager(
            string connectionString)
        {
            HasLoaded = true;
            CurrentValue = connectionString;
        }
        
        
        public bool HasLoaded { get; }
        
        public string CurrentValue { get; }
        
        
        public Task<string> Reload()
        {
            return Task.FromResult(CurrentValue);
        }
    }
}