using Rewind.Common;
using Rewind.Extensions.Persistence;
using Rewind.Extensions.Sync;
using Rewind.Store;

namespace Rewind.Extensions.Store
{
    public class ExtendedStateManager : IStateManager
    {
        public IPersistanceService? Persistence { get; set; }
        public ISyncService? SyncService { get; set; }

        public ExtendedStateManager(IPersistanceService persistence = null, ISyncService? syncService = null)
        {
            Persistence = persistence;
            SyncService = syncService;
        }

        public async ValueTask<Snapshot<TState>?> GetState<TState>(string name = "")
        {
            Snapshot<TState> snapshot = null;
            if (Persistence != null)
            {
                PersistenceKey pk = new PersistenceKey(HelperMethods.StoreType<TState>(), name);
                var state = await Persistence.GetStateAsync(pk);
                if (state != null)
                {
                    snapshot = state.ToSnapshot().ToSnapshot<TState>();
                }
            }

            return snapshot;
        }

        public async ValueTask<SerializableSnapshot?> GetState(StoreKey key)
        {
            SerializableSnapshot snapshot = null;
            if (Persistence != null)
            {
                PersistenceKey pk = new PersistenceKey(key);
                var state = await Persistence.GetStateAsync(pk);
                if (state != null)
                {
                    snapshot = state.ToSnapshot();
                }
            }


            return snapshot;
        }

        public async ValueTask<SerializableSnapshot?> GetState(string storeType, string stateName = "")
        {
            SerializableSnapshot snapshot = null;
            if (Persistence != null)
            {
                PersistenceKey pk = new PersistenceKey(storeType, stateName);
                var state = await Persistence.GetStateAsync(pk);
                if (state != null)
                {
                    snapshot = state.ToSnapshot();
                }
            }

            return snapshot;
        }

        public ValueTask<bool> HasState(StoreKey key)
        {
            if (Persistence == null)
                return ValueTask.FromResult(false);
            
            PersistenceKey pk = new PersistenceKey(key);

            return Persistence.HasStateAsync(pk);
        }

        public async ValueTask<bool> RemoveStatesUntil(StoreKey key, long version)
        {
            if (Persistence != null)
            {
                PersistenceKey pk = new PersistenceKey(key);
                await Persistence.RemoveStatesBeforeAsync(pk, version);
                return true;
            }
            return false;
        }

        public async ValueTask<bool> SetState(SerializableSnapshot snapshot)
        {
            if (Persistence != null)
            {
                PersistenceKey key = new PersistenceKey(snapshot.Key);
                var version = await Persistence.GetVersionAsync(key);
                if (snapshot.Version == version + 1)
                {
                    PersistenceData data = new PersistenceData(snapshot);
                    var result = await Persistence.SetStateAsync(data);
                }
            }

            if (SyncService != null)
            {
                await SyncService.UpdateRequest(snapshot);
            }

            return false;
        }

        public async ValueTask<long?> Version(StoreKey key)
        {
            if (Persistence != null)
            {
                PersistenceKey pk = new PersistenceKey(key);
                var version = await Persistence.GetVersionAsync(pk);
            }

            return null;
        }


    }
}
