using Rewind.Store;

namespace Rewind.Extensions.Store
{
    public class StateManager : IStateManager
    {
        public IStoreManager Stores { get; set; }

        public StateManager(IStoreManager storeManager)
        {
            Stores = storeManager;
        }

        public async ValueTask<Snapshot<TState>?> GetState<TState>(StoreKey key)
        {
            var store = await Stores.GetStore<TState>();
            Snapshot<TState>? snapshot = store?.Snapshot;
            return snapshot;
        }

        public async ValueTask<Snapshot<TState>?> GetState<TState>()
        {
            var store = await Stores.GetStore<TState>();
            Snapshot<TState>? snapshot = store?.Snapshot;
            return snapshot;
        }

        public async ValueTask<SerializableSnapshot?> GetState(StoreKey key)
        {
            var store = await Stores.GetStore(key.Type);
            SerializableSnapshot? snapshot = store?.GetSnapshot();
            return snapshot;
        }

        public async ValueTask<SerializableSnapshot?> GetState(string storeType)
        {
            var store = await Stores.GetStore(storeType);
            SerializableSnapshot? snapshot = store?.GetSnapshot();
            return snapshot;
        }

        public async ValueTask<bool> HasState(StoreKey key)
        {
            var store = await Stores.GetStore(key.Type);
            return store != null;
        }
        public ValueTask<bool> RemoveStatesUntil(StoreKey key, long version)
        {
            return ValueTask.FromResult(false);
        }

        public async ValueTask<bool> SetState(SerializableSnapshot snapshot)
        {
            var store = await Stores.GetStore(snapshot.Key.Type);
            if (store == null)
                return false;

            await store.SetSnapshot(snapshot);

            return true;
        }

        public async ValueTask<long?> Version(StoreKey key)
        {
            var store = await Stores.GetStore(key.Type);

            if (store == null)
                return null;

            return store.Version;
        }


    }
}
