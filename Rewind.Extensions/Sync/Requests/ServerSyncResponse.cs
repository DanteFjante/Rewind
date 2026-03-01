using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ServerSyncResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string? Error { get; set; }

        public StoreKey? StoreKey { get; set; }
        public Guid? InstanceId { get; set; }

        public string? UserId { get; set; }

        public SerializableSnapshot? Snapshot { get; set; }

        public const string InvokeKey = "ServerSyncResponse";
    }
}
