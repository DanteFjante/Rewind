using Rewind.Common;
using Rewind.Store;

namespace Rewind.Base.Store.Implementation
{
    public class BaseStoreManager : IStoreManager
    {
        public bool InitializeStores { get; private set; }
        public List<IStore> Stores { get; }


        public BaseStoreManager()
        {
            Stores = new();
        }

        public void EnableStoreInitialization()
        {
            InitializeStores = true;
        }

        public ValueTask<bool> AddStore(IStore store)
        {
            if (Stores.Any(x => x.Type == store.Type))
                return ValueTask.FromResult(false);

            Stores.Add(store);

            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> RemoveStore(string storeType)
        {
            if (!Stores.Any(x => x.Type == storeType))
            {
                return ValueTask.FromResult(false);
            }

            Stores.RemoveAll(x => x.Type == storeType);
            return ValueTask.FromResult(true);
        }

        public async ValueTask<IStore<TState>?> GetStore<TState>()
        {
            var store = await GetStore(HelperMethods.StoreType<TState>()) as IInitializableStore<TState>;

            if (store != null && InitializeStores)
                await store.InitializeAsync();

            return store;
        }
        public async ValueTask<IStore?> GetStore(string storeType)
        {
            var store = await GetStore(storeType) as IInitializableStore;

            if (store != null && InitializeStores)
                await store.InitializeAsync();

            return store;
        }

        public ValueTask<bool> HasStore<TState>() 
            => ValueTask.FromResult(Stores.Any(x => x.Type == HelperMethods.StoreType<TState>()));

        public ValueTask<long?> Version<TState>(string name = "")
        {
            if (!Stores.Any(x => x.Type == HelperMethods.StoreType<TState>()))
                return new((long?)null);
            return new(Stores.Max(x => x.GetSnapshot(name)?.Version ?? -1));
        }

        public ValueTask<IEnumerable<string>> GetStoreTypes()
        {
            return new(Stores.Select(x => x.Type).AsEnumerable());
        }
    }
}
