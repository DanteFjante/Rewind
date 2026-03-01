using Microsoft.AspNetCore.SignalR.Client;
using Rewind.Extensions.Users;
using Rewind.Sync.Client;
using Rewind.Sync.Requests;
namespace Rewind.Blazor.Sync
{
    public class ClientSyncConnection : IClientSyncConnection
    {
        public HubConnection Connection { get; set; }
        public UserService UserService { get; set; }

        public ClientSyncConnection(HubConnection connection, UserService userService)
        {
            Connection = connection;
            UserService = userService;
        }

        #region Incoming
        public IDisposable OnServerDispatch(Action<ServerDispatchRequest> onDispatch)
        {
            return Connection.On(ServerDispatchRequest.InvokeKey, onDispatch);
        }

        public IDisposable OnServerSync(Func<ServerSyncRequest, ClientSyncResponse> onSync)
        {
            return Connection.On(ServerSyncRequest.InvokeKey, onSync);
        }

        #endregion

        #region Outgoing
        public async ValueTask ClientUpdateAsync(ClientUpdateRequest request, CancellationToken ct = default)
        {
            if (Connection.State == HubConnectionState.Disconnected)
            {
                await Connection.StartAsync(ct);
            }

            await Connection.SendAsync(ClientUpdateRequest.InvokeKey, request, ct);
        }


        public async ValueTask<ServerSyncResponse> ClientRequestAsync(ClientSyncRequest request, CancellationToken ct = default)
        {
            if (Connection.State == HubConnectionState.Disconnected)
            {
                await Connection.StartAsync(ct);
            }
            ServerSyncResponse response = await Connection.InvokeAsync<ServerSyncResponse>(ClientSyncRequest.InvokeKey, request, ct);

            return response ?? new ServerSyncResponse() { Error = "Something went wrong with syncing with server"};
        }

        #endregion


    }
}
