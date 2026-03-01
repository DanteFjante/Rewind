using Rewind.Extensions.Sync;
using Rewind.Middleware;
using Rewind.Settings;

namespace Rewind.Sync
{
    public class SyncMiddleware<TState> : BaseMiddleware<TState>
    {
        public ISyncService SyncService;
        public SyncSettings SyncSettings { get; set; }

        public SyncMiddleware(SyncSettings syncSettings, ISyncService syncService)
        {
            SyncService = syncService;
            SyncSettings = syncSettings;
        }

        protected override async ValueTask BeforeInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            var state = await SyncService.SyncRequest(context.StoreKey, ct);
            if (state != null)
            {
                if (state.Version > context.Version)
                {
                    context.ApplySnapshot(state);
                }
            }

            await next(context, ct);
        }

        protected async override ValueTask AfterInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {

            await SyncService.UpdateRequest(context.ToSerializableSnapshot(), ct);

            await next(context, ct);
        }

        protected override async ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            await SyncService.UpdateRequest(context.ToSerializableSnapshot(), ct);

            await next(context, ct);
        }
    }
}
