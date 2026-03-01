using Rewind.Common;
using Rewind.Store;

namespace Rewind.Base.Store.Implementation
{
    public class BaseStateManager : IStateManager
    {
        public Dictionary<StoreKey, SerializableSnapshot> States { get; set; } = new();

        public ValueTask<Snapshot<TState>?> GetState<TState>(StoreKey key)
        {
            if (States.TryGetValue(key, out var snapshot))
            {
                return ValueTask.FromResult<Snapshot<TState>?>(snapshot.ToSnapshot<TState>());
            }
            return ValueTask.FromResult<Snapshot<TState>?>(null);
        }

        public ValueTask<Snapshot<TState>?> GetState<TState>()
        {
            StoreKey key = new StoreKey(HelperMethods.StoreType<TState>());
            return GetState<TState>(key);
        }

        public ValueTask<SerializableSnapshot?> GetState(StoreKey key)
        {
            if (States.TryGetValue(key, out var snapshot))
            {
                return ValueTask.FromResult<SerializableSnapshot?>(snapshot);
            }
            return ValueTask.FromResult<SerializableSnapshot?>(null);
        }

        public ValueTask<SerializableSnapshot?> GetState(string storeType)
        {
            StoreKey key = new StoreKey(storeType);
            return GetState(key);
        }

        public ValueTask<bool> HasState(StoreKey key)
        {
            return ValueTask.FromResult(States.ContainsKey(key));
        }


        public ValueTask<bool> RemoveStatesUntil(StoreKey key, long version)
        {
            return new(true);
        }

        public ValueTask<bool> SetState(SerializableSnapshot snapshot)
        {
            if (States.TryGetValue(snapshot.Key, out var current))
            {
                if(snapshot.Version != current.Version + 1)
                {
                    return new(false);
                }

            }

            States[snapshot.Key] = snapshot;
            return new(true);
        }

        public ValueTask<long?> Version(StoreKey key)
        {
            if (States.TryGetValue(key, out var state))
            {
                return ValueTask.FromResult((long?) state.Version);
            }

            return ValueTask.FromResult((long?) null);
        }
    }
}
