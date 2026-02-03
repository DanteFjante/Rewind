using Rewind.Store.Interface;

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
