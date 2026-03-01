using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ClientSyncResponse
    {
        public bool IsSuccess { get; set; } = false;

        public string? Error { get; set; }

        public string? UserId { get; set; }
        public StoreKey? StoreKey { get; set; }
        public Guid InstanceId { get; set; }

        public SerializableSnapshot? Snapshot { get; set; }

        public Snapshot<TState>? GetSnapshot<TState>() => Snapshot?.ToSnapshot<TState>();
    }
}
