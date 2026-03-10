using Rewind.Common;
using Rewind.Extensions.Persistence;
using Rewind.Server.Persistence;
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

        public async ValueTask<Snapshot<TState>?> GetState<TState>(string name = "")
        {
            PersistenceKey pk = new PersistenceKey(HelperMethods.StoreType<TState>(), name);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot().ToSnapshot<TState>();
        }

        public async ValueTask<SerializableSnapshot?> GetState(StoreKey key)
        {
            PersistenceKey pk = new PersistenceKey(key.Type, key.Name);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot();
        }

        public async ValueTask<SerializableSnapshot?> GetState(string storeType, string stateName = "")
        {
            PersistenceKey pk = new PersistenceKey(storeType, stateName);
            var store = await Storage.GetStateAsync(pk);

            return store?.ToSnapshot();
        }
    }
}
