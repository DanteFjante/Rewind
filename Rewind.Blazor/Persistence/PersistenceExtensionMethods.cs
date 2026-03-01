using Rewind.Blazor.Persistence;
using Rewind.Extensions.Persistence;
using Rewind.Store.Builders;

namespace Rewind.Blazor.Persistence
{
    public static class PersistenceExtensionMethods
    {
        public static IStoreBuilder<TState> AddLocalPersistence<TState>(
            this IStoreBuilder<TState> storeBuilder,
            string? storageKey = null,
            Action<PersistenceSettings>? options = null)
        {
            return Extensions.Persistence.PersistenceExtensionMethods.AddLocalPersistence<TState, LocalWebStorage>(storeBuilder, storageKey, options);
        }
    }
}
