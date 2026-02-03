using Microsoft.JSInterop;
using System.Globalization;
using System.Text.Json;

namespace Rewind.LocalStorage
{
    public class LocalWebStorage : ILocalStorage
    {
        public IJSRuntime JS { get; }

        public LocalWebStorage(IJSRuntime js)
        {
            this.JS = js;
        }
        private ValueTask SetItemAsync(string key, string value)
        {

            return JS.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public ValueTask SetItemAsync<T>(string key, T value)
        {

            if (value is IFormattable)
            {
                return SetItemAsync(key, (value as IFormattable)!.ToString(null, CultureInfo.InvariantCulture));
            }

            string toEnter = JsonSerializer.Serialize<T>(value);
            return SetItemAsync(key, toEnter);
        }


        public async ValueTask<T> GetItemAsync<T>(string key, T fallback = default!)
        {
            var retrieved = await JS.InvokeAsync<string>("localStorage.getItem", key);

            if (!string.IsNullOrWhiteSpace(retrieved))
            {
                try
                {
                    T? value = JsonSerializer.Deserialize<T>(retrieved);

                    if (value != null)
                        return value!;
                }
                catch { }
            }
            return fallback!;
        }

        public ValueTask DeleteItemAsync(string key)
        {
            return JS.InvokeVoidAsync("localStorage.removeItem", key);
        }

        public ValueTask ClearStorageAsync(string key)
        {
            return JS.InvokeVoidAsync("localStorage.clear");
        }

        public async ValueTask<bool> HasKeyAsync(string key)
        {
            var retrieved = await JS.InvokeAsync<string>("localStorage.getItem", key);
            return retrieved is not null;
        }

        public async ValueTask<IEnumerable<string>> GetKeys()
        {
            return await JS.InvokeAsync<string[]>("Object.keys", await JS.InvokeAsync<object>("localStorage"));
        }
    }
}
