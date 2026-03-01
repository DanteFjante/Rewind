using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Common;
using Rewind.Extensions.Store;
using Rewind.Middleware;

namespace Rewind.Store.Builders;

internal class StoreBuilder<TState> : IStoreBuilder<TState>
{
    readonly List<MiddlewareRegistration> _middleware;
    readonly List<OptionsRegistration> _options;
    readonly List<ServiceRegistration> _services;
    readonly List<Action<IServiceProvider, IInitializableStore<TState>>> _onStoreCreation;

    public Dictionary<string, object> Settings { get; }

    TState _initialState;
    Type _stateManagerType;
    Type _storeManagerType;

    public StoreBuilder(TState initialState)
    {
        _initialState = initialState;
        _stateManagerType = typeof(StateManager);
        _storeManagerType = typeof(StoreManager);

        Settings = new();
        _middleware = new();
        _options = new();
        _services = new();
        _onStoreCreation = new();
    }

    public IStoreBuilder<TState> SetStoreManager<StoreManager>() where StoreManager : IStoreManager
    {
        _storeManagerType = typeof(StoreManager);
        return this;
    }
    public IStoreBuilder<TState> SetStateManager<StateManager>() where StateManager : IStateManager
    {
        _stateManagerType = typeof(StateManager);
        return this;
    }


    public IStoreBuilder<TState> AddStoreDecorator(Action<IServiceProvider, IInitializableStore<TState>> storeDecorator)
    {
        _onStoreCreation.Add(storeDecorator);
        return this;
    }

    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(
        Func<IMiddlewareBuilder<TState, TMiddleware>, IMiddlewareBuilder<TState, TMiddleware>>? builder = null) 
        where TMiddleware : BaseMiddleware<TState>
    {
        MiddlewareBuilder<TState, TMiddleware> middlewareBuilder = new MiddlewareBuilder<TState, TMiddleware>();

        if (builder != null)
            middlewareBuilder = (MiddlewareBuilder<TState, TMiddleware>)builder(middlewareBuilder);

        _middleware.Add(middlewareBuilder.Build());

        return this;
    }

    private void SetupMiddleware(
        IServiceCollection sc, 
        IEnumerable<MiddlewareRegistration> registrations)
    {
        foreach (var factory in registrations)
        {
            if(factory.ProviderRegistration != null)
                factory.ProviderRegistration(sc);
        }
    }

    public IStoreBuilder<TState> AddOptions<TOptions>(
        Func<IOptionsBuilder<TState, TOptions>, IOptionsBuilder<TState, TOptions>>? builder = null) 
        where TOptions : class
    {
        OptionsBuilder<TState, TOptions> optionsBuilder = new OptionsBuilder<TState, TOptions>();

        if (builder != null)
            optionsBuilder = (OptionsBuilder<TState, TOptions>)builder(optionsBuilder);

        _options.Add(optionsBuilder.Build());

        return this;
    }

    private void SetupOptions(IServiceCollection sc, IEnumerable<OptionsRegistration> options)
    {
        foreach (var option in options)
        {
            if(option.ProviderRegistration != null)
                option.ProviderRegistration(sc);
        }
    }

    public IStoreBuilder<TState> AddService<TService>(
        Func<IServiceBuilder<TState, TService>, IServiceBuilder<TState, TService>>? builder = null)
        where TService : class
    {
        ServiceBuilder<TState, TService> serviceBuilder = new ServiceBuilder<TState, TService>();

        if (builder != null)
            serviceBuilder = (ServiceBuilder<TState, TService>)builder(serviceBuilder);

        _services.Add(serviceBuilder.Build());

        return this;
    }


    private void SetupServices(IServiceCollection sc, IEnumerable<ServiceRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            if(registration.ProviderRegistration != null)
                registration.ProviderRegistration(sc);
        }
    }

    public Func<IServiceProvider, IInitializableStore<TState>> Build(IServiceCollection sc)
    {
        
        SetupServices(sc, _services);

        SetupOptions(sc, _options);

        SetupMiddleware(sc, _middleware);

        var mwPipeline = BuildMiddlewarePipeline(_middleware);
        StoreKey key = new StoreKey(HelperMethods.StoreType<TState>(), "");

        sc.TryAddScoped(sp => StoreFactory.Create(_initialState, key, mwPipeline(sp)));
        sc.TryAddScoped<IStore<TState>>(sp => sp.GetRequiredService<IInitializableStore<TState>>());
        sc.AddScoped<IStore>(sp => sp.GetRequiredService<IInitializableStore<TState>>());
        sc.AddScoped<IInitializableStore>(sp => sp.GetRequiredService<IInitializableStore<TState>>());

        sc.TryAdd(new ServiceDescriptor(typeof(IStoreManager), _storeManagerType, ServiceLifetime.Scoped));
        sc.TryAdd(new ServiceDescriptor(typeof(IStateManager), _stateManagerType, ServiceLifetime.Scoped));

        return sp => sp.GetRequiredService<IInitializableStore<TState>>();
    }

    private Func<IServiceProvider, List<Func<BaseMiddleware<TState>>>> BuildMiddlewarePipeline(
    IEnumerable<MiddlewareRegistration> mw)
    {
        Func<IServiceProvider, List<Func<BaseMiddleware<TState>>>> middlewaresFactory = (sp) =>
        {
            List<Func<BaseMiddleware<TState>>> list = new();
            foreach (var reg in mw)
            {
                list.Add(() => (BaseMiddleware<TState>)reg.Factory(sp));
            }

            return list;
        };

        return middlewaresFactory;
    }

}
