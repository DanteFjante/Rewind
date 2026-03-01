using Rewind.Middleware;

namespace Rewind.Store.Builders;

public interface IStoreBuilder<TState>
{
    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(Func<IMiddlewareBuilder<TState, TMiddleware>, IMiddlewareBuilder<TState, TMiddleware>>? builder = null)
        where TMiddleware : BaseMiddleware<TState>;


    public IStoreBuilder<TState> AddOptions<TOptions>(Func<IOptionsBuilder<TState, TOptions>, IOptionsBuilder<TState, TOptions>>? builder = null)
        where TOptions : class;

    public IStoreBuilder<TState> AddService<TService>(Func<IServiceBuilder<TState, TService>, IServiceBuilder<TState, TService>>? builder = null)
        where TService : class;

    public IStoreBuilder<TState> AddStoreDecorator(Action<IServiceProvider, IInitializableStore<TState>> storeDecorator);

    public IStoreBuilder<TState> SetStoreManager<StoreManager>() where StoreManager : IStoreManager;
    public IStoreBuilder<TState> SetStateManager<StateManager>() where StateManager : IStateManager;
}
