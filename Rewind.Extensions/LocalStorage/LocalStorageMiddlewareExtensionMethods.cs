using Rewind.Common;
using Rewind.Store;

namespace Rewind.LocalStorage
{
    public static class LocalStorageMiddlewareExtensionMethods
    {
        public static IStoreBuilder<TState> AddPersistence<TState, TLocalStorage>(
            this IStoreBuilder<TState> storeBuilder,
            string? storageKey = null,
            Action<LocalStorageSettings>? options = null)
            where TLocalStorage : class, ILocalStorage
        {
            Action<LocalStorageSettings> opt;
            if (options != null)
            {
                opt = options;
            }
            else
            {
                opt = o => o.StorageKey ??= storageKey ?? HelperMethods.StoreName<TState>();
            }
            
            storeBuilder.AddOptions(opt);
            storeBuilder.AddService<ILocalStorage, TLocalStorage>();
            storeBuilder.AddMiddleware<LocalStorageMiddleware<TState>>();
            return storeBuilder;
        }
    }
}
