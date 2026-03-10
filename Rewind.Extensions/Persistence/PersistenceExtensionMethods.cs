using Rewind.Extensions.Persistence.Client;
using Rewind.Extensions.Store;
using Rewind.Store.Builders;

namespace Rewind.Extensions.Persistence
{
    public static class PersistenceExtensionMethods
    {
        public static IStoreBuilder<TState> AddPersistence<TState, TPersistenceService>(
            this IStoreBuilder<TState> storeBuilder,
            string? appSettingsKey = null,
            Action<PersistenceSettings>? options = null
            )
            where TPersistenceService : class, IPersistanceService
        {
            storeBuilder
                .AddOptions<PersistenceSettings>(b => appSettingsKey != null ? b.ReadFromSettings(appSettingsKey) : b)
                .AddService<IPersistanceService>(b => b.SetImplementationType<TPersistenceService>())
                .AddMiddleware<PersistenceMiddleware<TState>>();
            return storeBuilder;
        }

        public static IStoreBuilder<TState> AddLocalPersistence<TState, TRepo>(
    this IStoreBuilder<TState> storeBuilder,
    string? appSettingsKey = null,
    Action<PersistenceSettings>? options = null
    )
    where TRepo : class, ILocalRepo
        {
            storeBuilder
                .AddOptions<PersistenceSettings>(b => appSettingsKey != null ? b.ReadFromSettings(appSettingsKey) : b)
                .AddService<IPersistanceService>(b => b.SetImplementationType<LocalStorageService>())
                .AddService<ILocalRepo>(b => b.SetImplementationType<TRepo>())
                .AddMiddleware<PersistenceMiddleware<TState>>();
            return storeBuilder;
        }
    }
}
