using Microsoft.Extensions.Logging;
using Rewind.Common;
using Rewind.Middleware;

namespace Rewind.Logging
{
    public class LoggingMiddleware<TState> : BaseMiddleware<TState>
    {
        ILogger<LoggingMiddleware<TState>>? _logger { get; }
        Action<string> Info;
        Action<string> Warn;

        public LoggingMiddleware(ILogger<LoggingMiddleware<TState>>? logger = null)
        {
            _logger = logger;

            if (logger != null)
            {
                _logger!.LogInformation("Logger found. Setting up logging middleware with logging through logger.");
                Info = (s) => logger!.LogInformation(s);
                Warn = (s) => logger!.LogWarning(s);
            }
            else
            {
                Console.WriteLine("No logger found. Setting up logging middleware with logging through System Console.");
                Info = (s) => Console.WriteLine("Info: " + s);
                Warn = (s) => Console.WriteLine("Warning: " + s);
            }
        }


        protected override ValueTask BeforeInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            Info("Initializing Store" + HelperMethods.StoreType<TState>());
            return next(context, ct);
        }

        protected override ValueTask AfterInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            if (context.Blocked)
            {
                Warn($"Initializing store <{HelperMethods.StoreType<TState>()}> was Blocked, reason: {context.BlockedReason}");
            }
            else
            {
                Info($"Store Initialized for: <{HelperMethods.StoreType<TState>()}> with state: {context.State}");
            }

            return next(context, ct);
        }

        protected override ValueTask BeforeUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            Info($"Updating store: <{HelperMethods.StoreType<TState>()}> with state: {context.CurrentState}");
            return next(context, ct);
        }

        protected override ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            if (context.Blocked)
            {
                Warn($"Updating store <{HelperMethods.StoreType<TState>()}> was Blocked, reason: {context.BlockedReason}");
            }
            else
            {
                Info($"Updated store: <{HelperMethods.StoreType<TState>()}> from state {context.CurrentState} with new state {context.NextState}");
            }
            return next(context, ct);
        }
    }
}
