using Rewind.Sync.Requests;

namespace Rewind.Sync.Server
{
    public interface IServerSyncConnection
    {
        public ValueTask ServerDispatch(ServerDispatchRequest request, CancellationToken ct = default);

        public ValueTask<ClientSyncResponse> ServerSync(ServerSyncRequest request, CancellationToken ct = default);

    }
}
