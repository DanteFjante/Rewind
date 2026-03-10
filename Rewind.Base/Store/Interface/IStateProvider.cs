namespace Rewind.Store
{
    public interface IStateProvider
    {
        public ValueTask<Snapshot<TState>?> GetState<TState>(string name = "");
        public ValueTask<SerializableSnapshot?> GetState(StoreKey key);
        public ValueTask<SerializableSnapshot?> GetState(string storeType, string stateName = "");
    }
}
