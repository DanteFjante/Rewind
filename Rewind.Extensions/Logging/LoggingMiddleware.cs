using Rewind.Redux.Middleware;
using Microsoft.Extensions.Logging;

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
            Info("Initializing Store" + typeof(TState).FullName);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask AfterInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            if (context.Blocked)
            {
                Warn($"Initializing store <{typeof(TState).FullName}> was Blocked, reason: {context.BlockedReason}");
            }
            else
            {
                Info($"Store Initialized for: <{typeof(TState).FullName}> with state: {context.State}");
            }

            return ValueTask.CompletedTask;
        }

        protected override ValueTask BeforeUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            Info($"Updating store: <{typeof(TState).FullName}> with state: {context.CurrentState}");
            return ValueTask.CompletedTask;
        }

        protected override ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            if (context.Blocked)
            {
                Warn($"Updating store <{typeof(TState).FullName}> was Blocked, reason: {context.BlockedReason}");
            }
            else
            {
                Info($"Updated store: <{typeof(TState).FullName}> from state {context.CurrentState} with new state {context.NextState}");
            }
            return ValueTask.CompletedTask;
        }
    }
}
