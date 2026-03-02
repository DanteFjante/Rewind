using Rewind.Extensions.Persistence;

namespace Rewind.Server.Persistence
{
    public class ServerStorageService : IServerStorageService
    {
        private IUserServerRepository repo;

        public ServerStorageService(IUserServerRepository repo)
        {
            this.repo = repo;
        }

        public async ValueTask<HashSet<PersistenceKey>> GetKeysAsync()
        {
            return await repo.GetKeysAsync();
        }
        public async ValueTask<HashSet<string>> GetTypeKeysAsync(string type)
        {
            return await repo.GetTypeKeysAsync(type);
        }

        public async ValueTask<long?> GetVersionAsync(PersistenceKey key)
        {
            return await repo.GetVersionAsync(key) ?? -1;
        }

        public async ValueTask<long?> GetOldestVersionAsync(PersistenceKey key)
        {
            return await repo.GetOldestVersionAsync(key) ?? -1;
        }

        public ValueTask<bool> HasVersionAsync(PersistenceKey key, long version)
        {
            return repo.HasVersion(key, version);
        }

        public ValueTask<bool> HasStateAsync(PersistenceKey key)
        {
            return repo.HasState(key);
        }

        public async ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key)
        {
            var state = await repo.GetStateAsync(key);
            return (state)?.ToPersistenceData();
        }

        public async ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key, long version)
        {
            return (await repo.GetStateAsync(key, version))?.ToPersistenceData();
        }

        public async ValueTask<HashSet<PersistenceData>?> GetStatesAsync(PersistenceKey key)
        {
            var states = await repo.GetStatesAsync(key);
            return states.Select(x => x.ToPersistenceData()).ToHashSet();
        }

        public ValueTask<bool> RegisterStore(bool isSharedStore, PersistenceKey key)
        {
            return repo.CreateStoreAsync(key.Name, key.Type, isSharedStore);
        }

        public ValueTask<bool> SetStateAsync(PersistenceData data)
        {
            return repo.SetStateAsync(data);
        }

        public async ValueTask<bool> LoadVersionAsync(PersistenceKey key, long version)
        {
            var state = await repo.GetStateAsync(key, version);

            if (state == null)
                return false;

            var currentState = await repo.GetStateAsync(key);

            if (currentState == null)
                return false;

            return await repo.SetStateAsync(new PersistenceData(state.StoreType, state.StoreName, currentState.Version + 1, state.StateJson, DateTime.UtcNow, "Loaded version " + version));
        }
        public ValueTask<int> RemoveStatesBeforeAsync(PersistenceKey key, long version)
        {
            return repo.RemoveStatesBefore(key, version);
        }

        public ValueTask<bool> JoinBranch(string branchName)
        {
            return repo.JoinBranch(branchName);
        }

        public async ValueTask<HashSet<string>> GetBranches(PersistenceKey key)
        {
            var branches = await repo.GetBranches(key);

            return branches.Select(x => x.Name).ToHashSet();
        }
    }
}
