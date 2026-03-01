using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ServerSyncRequest
    {
        public StoreKey StoreKey { get; set; }
        public string? UserId { get; set; }
        public Guid? InstanceId { get; set; }

        public const string InvokeKey = "ServerSyncRequest";
    }
}
