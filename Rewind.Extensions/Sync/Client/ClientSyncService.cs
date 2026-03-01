using Rewind.Extensions.Users;
using Rewind.Store;
using Rewind.Sync.Client;
using Rewind.Sync.Requests;

namespace Rewind.Extensions.Sync.Client
{
    public class ClientSyncService : ISyncService
    {
        IClientSyncConnection connection;
        UserService userService;
        public ClientSyncService(IClientSyncConnection connection, UserService userService)
        {
            this.connection = connection;
            this.userService = userService;
        }

        public async ValueTask<SerializableSnapshot?> SyncRequest(StoreKey key, CancellationToken ct = default)
        {
            ClientSyncRequest request = new ClientSyncRequest()
            {
                StoreKey = key,
                UserId = userService.UserId
            };
            var response = await connection.ClientRequestAsync(request);
            return response?.Snapshot;
        }

        public ValueTask UpdateRequest(SerializableSnapshot update, CancellationToken ct = default)
        {
            ClientUpdateRequest request = new ClientUpdateRequest()
            {
                Snapshot = update,
                StoreKey = update.Key,
                UserId = userService.UserId
            };

            return connection.ClientUpdateAsync(request, ct);
        }
    }
}
