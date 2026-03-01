namespace Rewind.Store
{
    public interface IStateManager : IStateProvider
    {
        public ValueTask<bool> SetState(SerializableSnapshot snapshot);
        public ValueTask<bool> RemoveStatesUntil(StoreKey key, long version);
        public ValueTask<bool> HasState(StoreKey key);
        public ValueTask<long?> Version(StoreKey key);
    }
}
