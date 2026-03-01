using Microsoft.AspNetCore.SignalR;
using Rewind.Blazor.Sync;
using Rewind.Sync.Requests;
using Rewind.Sync.Server;

namespace Rewind.Server.Sync
{
    internal class ServerSyncConnection : IServerSyncConnection
    {
        public IHubContext<SyncHub> HubContext { get; }

        public ServerSyncConnection(IHubContext<SyncHub> hubContext)
        {
            HubContext = hubContext;
        }

        public async ValueTask ServerDispatch(ServerDispatchRequest dispatch, CancellationToken ct = default)
        {
            if (dispatch.StoreInstanceId != null)
                await HubContext.Clients.Group(dispatch.StoreInstanceId).SendAsync("ServerDispatch", dispatch, ct);
        }

        public async ValueTask<ClientSyncResponse> ServerSync(ServerSyncRequest request, CancellationToken ct = default)
        {
            var task = (HubContext.Clients.User(request.UserId) as ISingleClientProxy)?.InvokeAsync<ClientSyncResponse>(ServerSyncRequest.InvokeKey, request, ct);
            if (task != null)
            {
                ClientSyncResponse response = await task;

            }

            return new ClientSyncResponse() { Error = "Something went wrong with syncing with client." };
        }
    }
}
