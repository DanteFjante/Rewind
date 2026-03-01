using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ClientUpdateRequest
    {
        public string UserId { get; set; }
        public StoreKey StoreKey { get; set; }

        public SerializableSnapshot Snapshot { get; set; }

        public Snapshot<TState> GetSnapshot<TState>() => Snapshot.ToSnapshot<TState>();

        public const string InvokeKey = "OnClientUpdate";
    }
}
