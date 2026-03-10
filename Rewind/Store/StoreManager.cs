using Microsoft.Extensions.DependencyInjection;
using Rewind.Store;

namespace Rewind.Extensions.Store
{
    public class StoreManager : IStoreManager
    {
        public bool InitializeStore { get; private set; }
        
        private IServiceProvider sp { get; }

        public StoreManager(IServiceProvider sp)
        {
            this.sp = sp;
        }

        public ValueTask<bool> AddStore(IStore store)
        {
            return ValueTask.FromResult(false);
        }

        public ValueTask<bool> RemoveStore(string storeType)
        {
            return ValueTask.FromResult(false);
        }

        public void EnableStoreInitialization()
        {
            InitializeStore = true;
        }

        public async ValueTask<IStore<TState>?> GetStore<TState>()
        {
            var store = sp.GetService<IInitializableStore<TState>>();

            if (InitializeStore && store != null)
                await store.InitializeAsync();

            return store;
        }

        public async ValueTask<IStore?> GetStore(string storeType)
        {
            var stores = sp.GetServices<IInitializableStore>();

            var store = stores.FirstOrDefault(x => x.Type == storeType);

            if (InitializeStore && store != null)
                await store.InitializeAsync();

            return store;
        }

        public ValueTask<bool> HasStore<TState>()
        {
            return ValueTask.FromResult(sp.GetService<IStore<TState>>() != null);
        }

        public ValueTask<long?> Version<TState>(string key = "")
        {
            IStore<TState>? store = sp.GetService<IStore<TState>>();

            return ValueTask.FromResult(store?.GetSnapshot(key)?.Version ?? null);
        }

        public ValueTask<IEnumerable<string>> GetStoreTypes()
        {
            return new(sp.GetServices<IStore>().Select(x => x.Type));
        }
    }
}
