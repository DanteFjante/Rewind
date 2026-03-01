namespace Rewind.Extensions.Persistence.Client;

public interface ILocalRepo
{
    public ValueTask<HashSet<PersistenceKey>> GetKeysAsync();
    public ValueTask<HashSet<string>> GetTypeKeysAsync(string type);


    public ValueTask<long?> GetVersionAsync(PersistenceKey key);

    public ValueTask<long?> GetOldestVersionAsync(PersistenceKey key);

    public ValueTask<bool> HasVersionAsync(PersistenceKey key, long version);

    public ValueTask<bool> HasStateAsync(PersistenceKey key);

    public ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key);

    public ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key, long version);

    public ValueTask<HashSet<PersistenceData>?> GetStatesAsync(PersistenceKey key);

    public ValueTask<bool> SetStateAsync(PersistenceData item);

    public ValueTask<int> RemoveStatesUntilAsync(PersistenceKey key, long version);

    public ValueTask<bool> ClearStorageAsync();
}
