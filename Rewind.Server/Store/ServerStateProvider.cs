using Rewind.Common;
using Rewind.Extensions.Persistence;
using Rewind.Extensions.Persistence.Server;
using Rewind.Store;

namespace Rewind.Server.Store
{
    public class ServerStateProvider : IStateProvider
    {
        public bool InitializeStores { get; set; }

        public IServerStorageService Storage { get; }


        public ServerStateProvider(IServerStorageService serverStorageService)
        {
            Storage = serverStorageService;
            
        }

        public void EnableStoreInitialization()
        {
            InitializeStores = true;
        }

        public async ValueTask<Snapshot<TState>?> GetState<TState>(StoreKey key)
        {
            PersistenceKey pk = new PersistenceKey(key);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot().ToSnapshot<TState>();
        }

        public async ValueTask<Snapshot<TState>?> GetState<TState>()
        {
            PersistenceKey pk = new PersistenceKey(HelperMethods.StoreType<TState>());
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot().ToSnapshot<TState>();
        }

        public async ValueTask<SerializableSnapshot?> GetState(StoreKey key)
        {
            PersistenceKey pk = new PersistenceKey(key.Type, key.Name);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot();
        }

        public async ValueTask<SerializableSnapshot?> GetState(string storeType)
        {
            PersistenceKey pk = new PersistenceKey(storeType);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot();
        }
    }
}
