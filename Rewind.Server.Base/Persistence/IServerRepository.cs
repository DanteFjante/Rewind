using Rewind.Extensions.Persistence;
using Rewind.Server.Base.Store;

namespace Rewind.Server.Base.Persistence
{
    public interface IServerRepository
    {
        public ValueTask<bool?> IsPublicAsync(PersistenceKey key);
        public ValueTask<HashSet<PersistenceKey>> GetKeysAsync();
        public ValueTask<HashSet<string>> GetTypeKeysAsync(string type);
        public ValueTask<HashSet<ServerBranch>> GetBranches(PersistenceKey key);

        public ValueTask<long?> GetVersionAsync(PersistenceKey key);
        public ValueTask<long?> GetOldestVersionAsync(PersistenceKey key);
        public ValueTask<bool> HasVersion(PersistenceKey key, long version);
        public ValueTask<bool> HasState(PersistenceKey key);


        public ValueTask<ServerState?> GetStateAsync(PersistenceKey key, bool includeLinks = false);
        public ValueTask<ServerState?> GetStateAsync(PersistenceKey key, long version, bool includeLinks = false);
        public ValueTask<HashSet<ServerState>> GetStatesAsync(PersistenceKey key, bool includeLinks = false);

        public ValueTask<bool> SetStateAsync(PersistenceData data);

        public ValueTask<ServerStore?> GetStoreAsync(PersistenceKey key);
        public ValueTask<HashSet<ServerStore>> GetStoresAsync(IEnumerable<PersistenceKey> keys);
        
        public ValueTask<bool> CreateStoreAsync(string storeType, string storeName, bool isPublic);
        public ValueTask<bool> CreateInitialStateAsync(bool isPublic, PersistenceData data);
        public ValueTask<bool> CreateInitialStateAsync(PersistenceData data);

        public ValueTask<int> RemoveStatesBefore(PersistenceKey key, long version);

    }
}
