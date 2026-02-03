namespace Rewind.LocalStorage
{
    public interface ILocalStorage
    {
        public ValueTask SetItemAsync<T>(string key, T item);

        public ValueTask<T> GetItemAsync<T>(string key, T fallback = default!);

        public ValueTask DeleteItemAsync(string key);

        public ValueTask ClearStorageAsync(string key);

        public ValueTask<bool> HasKeyAsync(string key);

        public ValueTask<IEnumerable<string>> GetKeys();
    }
}
