using Rewind.Extensions.Sync;
using Rewind.Store;
using Rewind.Sync.Requests;

namespace Rewind.Server.Sync
{
    public class ServerSyncService : ISyncService
    {
        IServerSyncConnection connection;

        public ServerSyncService(IServerSyncConnection connection)
        {
            this.connection = connection;
        }

        public async ValueTask<SerializableSnapshot?> SyncRequest(StoreKey key, CancellationToken ct = default)
        {
            ServerSyncRequest request = new ServerSyncRequest()
            {
                StoreKey = key,
            };

            var response = await connection.ServerSync(request, ct);

            return response?.Snapshot;
        }

        public async ValueTask UpdateRequest(SerializableSnapshot update, CancellationToken ct = default)
        {
            ServerDispatchRequest request = new ServerDispatchRequest()
            {
                Snapshot = update,
                StoreKey = update.Key
            };
            await connection.ServerDispatch(request, ct);
        }
    }
}
