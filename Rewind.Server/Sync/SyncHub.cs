using Microsoft.AspNetCore.SignalR;
using Rewind.Server.Persistence;
using Rewind.Server.Users;
using Rewind.Store;
using Rewind.Sync.Requests;

namespace Rewind.Server.Sync
{

    internal class SyncHub : Hub
    {

        IStateManager stateManager;
        UserContext? user;
        IServerStorageService storageService;
        public SyncHub(IStateManager stateManager, IServerStorageService storageService, UserContext? user = null)
        {
            this.stateManager = stateManager;
            this.storageService = storageService;
            this.user = user;

        }

        public async Task<ServerSyncResponse> OnClientSync(ClientSyncRequest request)
        {
            SetUser();

            var state = await stateManager.GetState(request.StoreKey);

            ServerSyncResponse response;
            if (state != null)
            {
                response = new ServerSyncResponse()
                {
                    IsSuccess = true,
                    StoreKey = state.Key,
                    Snapshot = state
                };
            }
            else
            {
                response = new ServerSyncResponse() { Error = "Something went wrong when client syncing with server." };
            }
            return response;
        }

        public async Task OnClientUpdate(ClientUpdateRequest request)
        {
            SetUser();
            await stateManager.SetState(request.Snapshot);
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {

        }

        public async override Task OnConnectedAsync()
        {
            SetUser();
        }

        private void SetUser()
        {
            if (user != null)
            {
                user.UserId = Context.UserIdentifier;
                user.IsAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;
                user.Principal = Context.User;
            }
        }

    }
}
