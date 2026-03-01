using Rewind.Middleware;
using Rewind.Store;

namespace Rewind.Extensions.Store
{
    public class StateMiddleware<TState> : BaseMiddleware<TState>
    {
        public IStateManager StateManager { get; set; }

        public StateMiddleware(IStateManager stateManager)
        {
            StateManager = stateManager;
        }

        protected override async ValueTask BeforeInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            var state = await StateManager.GetState(context.StoreKey);

            if(state != null)
                context.ApplySnapshot(state);

            await next(context, ct);
        }

        protected async override ValueTask AfterInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            await StateManager.SetState(context.Snapshot.ToSerializableSnapshot());
        }

        protected async override ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            await StateManager.SetState(context.Snapshot.ToSerializableSnapshot());
        }
    }
}
