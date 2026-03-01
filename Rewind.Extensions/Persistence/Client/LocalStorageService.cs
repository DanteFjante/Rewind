namespace Rewind.Extensions.Persistence.Client
{
    internal class LocalStorageService : ILocalStorageService
    {
        public ILocalRepo Repo { get; }

        public LocalStorageService(ILocalRepo repo)
        {
            Repo = repo;
        }

        public ValueTask<bool> ClearStorageAsync()
        {
            return Repo.ClearStorageAsync();
        }

        public ValueTask<HashSet<PersistenceKey>> GetKeysAsync()
        {
            return Repo.GetKeysAsync();
        }
        public ValueTask<HashSet<string>> GetTypeKeysAsync(string type)
        {
            return Repo.GetTypeKeysAsync(type);
        }

        public ValueTask<long?> GetVersionAsync(PersistenceKey key)
        {
            return Repo.GetVersionAsync(key);
        }

        public ValueTask<long?> GetOldestVersionAsync(PersistenceKey key)
        {
            return Repo.GetOldestVersionAsync(key);
        }

        public ValueTask<bool> HasVersionAsync(PersistenceKey key, long version)
        {
            return Repo.HasVersionAsync(key, version);
        }

        public ValueTask<bool> HasStateAsync(PersistenceKey key)
        {
            return Repo.HasStateAsync(key);
        }

        public ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key)
        {
            return Repo.GetStateAsync(key);
        }

        public ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key, long version)
        {
            return Repo.GetStateAsync(key, version);
        }

        public ValueTask<HashSet<PersistenceData>?> GetStatesAsync(PersistenceKey key)
        {
            return Repo.GetStatesAsync(key);
        }

        public ValueTask<bool> SetStateAsync(PersistenceData item)
        {
            return Repo.SetStateAsync(item);
        }

        public async ValueTask<bool> LoadVersionAsync(PersistenceKey key, long version)
        {
            var state = await Repo.GetStateAsync(key, version);

            if (state == null)
                return false;

            long? currenVersion = await GetVersionAsync(key);

            var newState = new PersistenceData(state, currenVersion.Value + 1, $"Rolled back to version: {version}");

            return await Repo.SetStateAsync(newState);
        }

        public ValueTask<int> RemoveStatesBeforeAsync(PersistenceKey key, long version)
        {
            return Repo.RemoveStatesUntilAsync(key, version);
        }
    }
}
