using Rewind.Sync.Requests;

namespace Rewind.Sync.Client
{
    public interface IClientSyncConnection
    {
        public IDisposable OnServerDispatch(Action<ServerDispatchRequest> onDispatch);
        public IDisposable OnServerSync(Func<ServerSyncRequest, ClientSyncResponse> onSync);

        public ValueTask<ServerSyncResponse> ClientRequestAsync(ClientSyncRequest request, CancellationToken ct = default);

        public ValueTask ClientUpdateAsync(ClientUpdateRequest request, CancellationToken ct = default);


    }
}
