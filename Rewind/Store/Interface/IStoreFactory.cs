using Microsoft.Extensions.Options;
using Rewind.Middleware;

namespace Rewind.Store;

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
