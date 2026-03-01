using Rewind.Store;

namespace Rewind.Middleware
{
    public class UpdateMiddlewareContext<TState>
    {
        public Func<TState, TState> Reducer { get; set; }
        public StoreKey StoreKey { get; }
        public long Version { get; }
        public DateTime At { get; }

        public bool Blocked { get; private set; }
        public string? BlockedReason { get; private set; }
        public string Reason { get; set; }

        public TState CurrentState { get; internal set; }
        public TState? NextState { get; internal set; }

        public UpdateMiddlewareContext(Func<TState, TState> reducer, TState state, StoreKey key, long version, string reason)
        {
            Reducer = reducer;
            CurrentState = state;

            Version = version;
            Reason = reason;
            StoreKey = key;
            At = DateTime.UtcNow;
        }

        public void Block(string reason)
        {
            Blocked = true;
            BlockedReason = reason;
        }

        public Snapshot<TState> Snapshot => new Snapshot<TState>(StoreKey, NextState!, Version, At, Reason);

        public SerializableSnapshot ToSerializableSnapshot()
        {
            return SerializableSnapshot.FromSnapshot(Snapshot);
        }
    }
}
