using Rewind.Store;

namespace Rewind.Sync.Requests
{
    public class ServerDispatchRequest
    {
        public string? BranchName { get; set; }

        public StoreKey StoreKey { get; set; }

        public SerializableSnapshot Snapshot { get; set; }

        public const string InvokeKey = "SeverDispatchRequest";
    }
}
