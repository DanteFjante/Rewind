using Rewind.Redux.Middleware;
using Rewind.Redux.Store.Interface;
using Rewind.Redux.Store.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rewind.Logging
{
    public static class LoggingMiddlewareExtensionMethods
    {
        public static IStoreBuilder<TState> AddLogging<TState>(this IStoreBuilder<TState> storeBuilder)
        {
            return storeBuilder.AddMiddleware<LoggingMiddleware<TState>>();
        }
    }
}
