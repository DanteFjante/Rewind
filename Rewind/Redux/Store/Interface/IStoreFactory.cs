using Microsoft.Extensions.Options;
using Rewind.Redux.Middleware;

namespace Rewind.Redux.Store.Interface
{
    public interface IStoreFactory<TState>
    {

        public TMiddleware? GetMiddleware<TMiddleware>()
            where TMiddleware : BaseMiddleware<TState>;

        public TService? GetService<TService>()
            where TService : class;

        public TOptions? GetOptions<TOptions>()
            where TOptions : class;
        public IOptionsMonitor<TOptions>? GetOptionsMonitor<TOptions>()
            where TOptions : class;
    }
}
