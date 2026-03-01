using Rewind.Store;

namespace Rewind.Extensions.Sync
{
    public interface ISyncService
    {
        public ValueTask UpdateRequest(SerializableSnapshot update, CancellationToken ct = default);
        public ValueTask<SerializableSnapshot?> SyncRequest(StoreKey key, CancellationToken ct = default);
    }
}
