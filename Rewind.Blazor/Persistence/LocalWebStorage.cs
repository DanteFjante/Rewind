using Microsoft.JSInterop;
using Rewind.Extensions.Persistence;
using Rewind.Extensions.Persistence.Client;
using System.Text.Json;

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

            Console.WriteLine($"Got {keys.Count()} keys");

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

            LocalStorageState? state = null;
            if (stateKey != null)
            {
                state = await GetItemAsync<LocalStorageState>(stateKey);

            }

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


            Console.WriteLine($"Got {latest} latest");

            if (latest != null && item.Version != latest + 1)
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
            // length is a property -> easiest is eval
            var length = await JS.InvokeAsync<int>("eval", "window.localStorage.length");

            var keys = new List<LocalStorageKey>(length);
            for (var i = 0; i < length; i++)
            {
                // localStorage.key(i) is a function -> callable directly
                var key = await JS.InvokeAsync<string>("localStorage.key", i);
                if (key is not null)
                {
                    LocalStorageKey? deserialized = null;
                    try
                    {
                        deserialized = JsonSerializer.Deserialize<LocalStorageKey>(key);
                    }
                    catch { }
                    if (deserialized != null)
                    {
                        keys.Add(deserialized);
                    }
                }
            }
            return keys;
        }

        private async ValueTask<TItem?> GetItemAsync<TItem>(LocalStorageKey key)
        {
            var jsonKey = JsonSerializer.Serialize(key);
            var result = await JS.InvokeAsync<string>("localStorage.getItem", jsonKey);
            var state = JsonSerializer.Deserialize<TItem>(result);
            return state;
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

        private ValueTask SetItemAsync(LocalStorageKey key, LocalStorageState state)
        {
            var jsonKey = JsonSerializer.Serialize(key);
            var jsonItem = JsonSerializer.Serialize(state);
            return JS.InvokeVoidAsync("localStorage.setItem", jsonKey, jsonItem);
        }

        private ValueTask SetItemsAsync(Dictionary<LocalStorageKey, object> items)
        {

            List<Task> tasks = new();
            foreach (var item in items)
            {
                var jsonKey = JsonSerializer.Serialize(item.Key);
                var jsonItem = JsonSerializer.Serialize(item.Value);
                tasks.Add(JS.InvokeVoidAsync("localStorage.setItem", jsonKey, jsonItem).AsTask());
            }
            return new ValueTask(Task.WhenAll(tasks));
        }

        private ValueTask RemoveItemAsync(LocalStorageKey key)
        {
            string jsonKey = JsonSerializer.Serialize(key);
            return JS.InvokeVoidAsync("localStorage.removeItem", jsonKey);
        }

        private ValueTask RemoveItemsAsync(IEnumerable<LocalStorageKey> keys)
        {
            List<Task> tasks = new();
            foreach (var key in keys)
            {
                string jsonKey = JsonSerializer.Serialize(key);
                tasks.Add(JS.InvokeVoidAsync("localStorage.removeItem", jsonKey).AsTask());
            }
            return new ValueTask(Task.WhenAll(tasks));
        }
        #endregion
    }
}
