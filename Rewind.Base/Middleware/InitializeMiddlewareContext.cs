using Rewind.Store;

namespace Rewind.Middleware
{
    public class InitializeMiddlewareContext<TState>
    {
        public StoreKey StoreKey { get; }

        public TState State { get; set; }

        public long Version { get; set; } = 0;

        public DateTime At { get; set; } = DateTime.UtcNow;

        public string Reason { get; set; } = "";

        public string? BlockedReason { get; private set; }

        public bool Blocked { get; private set; }

        public void Block(string reason)
        {
            Blocked = true;
            BlockedReason = reason;
        }

        public InitializeMiddlewareContext(StoreKey storeKey, TState state)
        {
            StoreKey = storeKey;
            State = state;
        }

        public Snapshot<TState> Snapshot => new Snapshot<TState>(StoreKey, State, Version, At, Reason);

        public SerializableSnapshot ToSerializableSnapshot()
        {
            return SerializableSnapshot.FromSnapshot(Snapshot);
        }

        public void ApplySnapshot(Snapshot<TState> snapshot)
        {
            State = snapshot.State;
            Version = snapshot.Version;
            At = snapshot.UpdatedAt;
            Reason = snapshot.Reason ?? "";
        }

        public void ApplySnapshot(SerializableSnapshot snapshot) => ApplySnapshot(snapshot.ToSnapshot<TState>());
    }
}
