using Microsoft.Extensions.DependencyInjection;
using Rewind.Middleware;

namespace Rewind.Store;

public interface IStoreBuilder<TState>
{
    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TMiddleware> factory)
        where TMiddleware : BaseMiddleware<TState>;

    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(
        ServiceLifetime serviceLifetime)
        where TMiddleware : BaseMiddleware<TState>;

    public IStoreBuilder<TState> AddMiddleware<TMiddleware>()
        where TMiddleware : BaseMiddleware<TState>;

    public IStoreBuilder<TState> AddOptions<TOptions>(
        Action<TOptions>? dynamicSettings = null)
        where TOptions : class;
    public IStoreBuilder<TState> AddOptions<TOptions>(
        Action<IServiceCollection> setup,
        Func<IServiceProvider, IStoreFactory<TState>, TOptions> factory)
        where TOptions : class;

    public IStoreBuilder<TState> AddService<TService>()
        where TService : class;
    public IStoreBuilder<TState> AddService<TService>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TService> factory) 
        where TService : class;
    public IStoreBuilder<TState> AddService<TService>(ServiceLifetime lifetime) 
        where TService : class;
    public IStoreBuilder<TState> AddService<TService, TImpl>()
        where TService : class
        where TImpl : class, TService;
    public IStoreBuilder<TState> AddService<TService, TImpl>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TService> factory) 
        where TService : class
        where TImpl : class, TService;
    public IStoreBuilder<TState> AddService<TService, TImpl>(ServiceLifetime lifetime) 
        where TService : class
        where TImpl : class, TService;



}
