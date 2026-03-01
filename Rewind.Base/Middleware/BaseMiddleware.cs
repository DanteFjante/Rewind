namespace Rewind.Middleware
{
    public abstract class BaseMiddleware<TState>
    {
        public delegate ValueTask UpdateNextAsync(UpdateMiddlewareContext<TState> context, CancellationToken ct);
        public delegate ValueTask InitNextAsync(InitializeMiddlewareContext<TState> context, CancellationToken ct);

        protected virtual ValueTask BeforeInitializeStore(
            InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask AfterInitializeStore(
            InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask BeforeUpdate(
            UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask AfterUpdate(
            UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct) => ValueTask.CompletedTask;
    
        internal async ValueTask InitializeAsync(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            await BeforeInitializeStore(context, next, ct);
            if(!context.Blocked) await next(context, ct);
            await AfterInitializeStore(context, next, ct);
        }

        internal async ValueTask UpdateAsync(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            await BeforeUpdate(context, next, ct);
            if (!context.Blocked) await next(context, ct);
            await AfterUpdate(context, next, ct);
        }
    }
}
