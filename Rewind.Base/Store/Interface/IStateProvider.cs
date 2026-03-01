namespace Rewind.Store
{
    public interface IStateProvider
    {
        public ValueTask<Snapshot<TState>?> GetState<TState>(StoreKey key);
        public ValueTask<Snapshot<TState>?> GetState<TState>();
        public ValueTask<SerializableSnapshot?> GetState(StoreKey key);
        public ValueTask<SerializableSnapshot?> GetState(string storeType);
    }
}
