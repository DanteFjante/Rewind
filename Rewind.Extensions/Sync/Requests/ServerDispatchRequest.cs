using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ServerDispatchRequest
    {
        public string? UserId { get; set; }
        public string? StoreInstanceId { get; set; }

        public StoreKey StoreKey { get; set; }

        public SerializableSnapshot Snapshot { get; set; }

        public const string InvokeKey = "SeverDispatchRequest";
    }
}
