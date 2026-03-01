using Microsoft.JSInterop;
using Rewind.Extensions.Persistence;
using Rewind.Extensions.Persistence.Client;

namespace Rewind.Blazor.Persistence
{
    public class LocalWebStorage : ILocalRepo
    {
        public IJSRuntime JS { get; }

        public LocalWebStorage(IJSRuntime js)
        {
            this.JS = js;
        }

        public async ValueTask<HashSet<PersistenceKey>> GetKeysAsync()
        {
            var keys = await GetAllKeysAsync();
            return keys.Select(x => x.ToPersistenceKey()).Distinct().ToHashSet();
        }
        public async ValueTask<HashSet<string>> GetTypeKeysAsync(string type)
        {
            var keys = await GetAllKeysAsync();
            return keys.Where(x => x.Type == type).Select(x => x.Name).Distinct().ToHashSet();
        }

        public async ValueTask<bool> HasStateAsync(PersistenceKey key)
        {
            var keys = await GetAllKeysAsync();
            return keys.Any(x => x.Type == key.Type && x.Name == key.Name);
        }
        public async ValueTask<bool> HasVersionAsync(PersistenceKey key, long version)
        {
            var keys = await GetAllKeysAsync();

            return keys.Any(x => x.Name == key.Name && x.Type == key.Type && x.Version == version);
        }
        public async ValueTask<long?> GetVersionAsync(PersistenceKey key)
        {
            var keys = await GetAllKeysAsync();

            var relevantKeys = keys.Where(x => x.Name == key.Name && x.Type == key.Type);
            if (!relevantKeys.Any())
                return null;

            return relevantKeys.Max(x => x.Version);
        }

        public async ValueTask<long?> GetOldestVersionAsync(PersistenceKey key)
        {
            var keys = await GetAllKeysAsync();

            var relevantKeys = keys.Where(x => x.Name == key.Name && x.Type == key.Type);
            if (!relevantKeys.Any())
                return null;

            return relevantKeys.Min(x => x.Version);
        }

        public async ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key)
        {
            var keys = await GetAllKeysAsync();

            var stateKey = keys.Where(x => x.Name == key.Name && x.Type == key.Type).MaxBy(x => x.Version);

            if (stateKey == null)
                return null;

            var state = await GetItemAsync<LocalStorageState>(stateKey);

            return state?.ToPersistenceData();
        }

        public async ValueTask<PersistenceData?> GetStateAsync(PersistenceKey key, long version)
        {

            var keys = await GetAllKeysAsync();

            var stateKey = keys.FirstOrDefault(x => x.Name == key.Name && x.Type == key.Type && x.Version == version);

            if (stateKey == null)
                return null;

            var state = await GetItemAsync<LocalStorageState>(stateKey);

            return state?.ToPersistenceData();
        }

        public async ValueTask<HashSet<PersistenceData>?> GetStatesAsync(PersistenceKey key)
        {
            var keys = await GetAllKeysAsync();

            var stateKeys = keys.Where(x => x.Name == key.Name && x.Type == key.Type);

            var states = await GetItemsAsync<LocalStorageState>(stateKeys);

            if (states == null)
                return null;

            return states!.Where(x => x is not null).Select(x => x!.ToPersistenceData()).ToHashSet();
        }

        public async ValueTask<bool> SetStateAsync(PersistenceData item)
        {

            var latest = await GetVersionAsync(item.ToKey());

            if (latest != -1 && item.Version != latest + 1)
                return false;

            await SetItemAsync(new LocalStorageKey(item.Name, item.Type, item.Version), new LocalStorageState(item));
            return true;
        }

        public async ValueTask<int> RemoveStatesUntilAsync(PersistenceKey key, long version)
        {
            var keys = await GetAllKeysAsync();
            var removeKeys = keys.Where(x => x.Type == key.Type && x.Name == x.Name && x.Version < version);

            await RemoveItemsAsync(removeKeys);

            return removeKeys.Count();
        }

        public async ValueTask<bool> ClearStorageAsync()
        {
            await JS.InvokeVoidAsync("localStorage.clear");
            return true;
        }


        #region Javascript interface
        private async ValueTask<IEnumerable<LocalStorageKey>> GetAllKeysAsync()
        {
            return await JS.InvokeAsync<LocalStorageKey[]>("Object.keys", await JS.InvokeAsync<object>("localStorage"));
        }

        private ValueTask<TItem?> GetItemAsync<TItem>(LocalStorageKey key)
        {
            return JS.InvokeAsync<TItem?>("localStorage.getItem", key);
        }

        private async ValueTask<IEnumerable<TItem?>> GetItemsAsync<TItem>(IEnumerable<LocalStorageKey> keys)
        {
            List<Task<TItem?>> tasks = new();
            foreach (var key in keys)
            {
                tasks.Add(GetItemAsync<TItem>(key).AsTask());
            }

            var items = await Task.WhenAll(tasks);

            return items;
        }

        private ValueTask SetItemAsync(LocalStorageKey key, object item)
        {
            return JS.InvokeVoidAsync("localStorage.setItem", key, item);
        }

        private ValueTask SetItemsAsync(Dictionary<LocalStorageKey, object> items)
        {

            List<Task> tasks = new();
            foreach (var item in items)
            {
                tasks.Add(JS.InvokeVoidAsync("localStorage.setItem", item.Key, item.Value).AsTask());
            }
            return new ValueTask(Task.WhenAll(tasks));
        }

        private ValueTask RemoveItemAsync(LocalStorageKey key)
        {
            return JS.InvokeVoidAsync("localStorage.removeItem", key);
        }

        private ValueTask RemoveItemsAsync(IEnumerable<LocalStorageKey> keys)
        {
            List<Task> tasks = new();
            foreach (var key in keys)
            {
                tasks.Add(JS.InvokeVoidAsync("localStorage.removeItem", key).AsTask());
            }
            return new ValueTask(Task.WhenAll(tasks));
        }
        #endregion
    }
}
