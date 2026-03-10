using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Middleware;
using Rewind.Store.Internal.Registrations;

namespace Rewind.Store.Builders;

internal class StoreBuilder<TState> : IStoreBuilder<TState>
    where TState : class
{
    readonly List<MiddlewareRegistration> _middleware;
    readonly List<OptionsRegistration> _options;
    readonly List<ServiceRegistration> _services;
    readonly List<Action<IServiceProvider, IInitializableStore<TState>>> _onStoreCreation;

    public Dictionary<string, object> Settings { get; }

    Func<TState> _initialState;

    public StoreBuilder(Func<TState> initialState)
    {
        _initialState = initialState;

        Settings = new();
        _middleware = new();
        _options = new();
        _services = new();
        _onStoreCreation = new();
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
    private void SetupMiddleware(IServiceCollection sc, IEnumerable<MiddlewareRegistration> registrations)
    {
        foreach (var factory in registrations)
        {
            if (factory.ProviderRegistration != null)
                factory.ProviderRegistration(sc);
        }
    }

    private void SetupOptions(IServiceCollection sc, IEnumerable<OptionsRegistration> options)
    {
        foreach (var option in options)
        {
            if (option.ProviderRegistration != null)
                option.ProviderRegistration(sc);
        }
    }
    private void SetupServices(IServiceCollection sc, IEnumerable<ServiceRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            if(registration.ProviderRegistration != null)
                registration.ProviderRegistration(sc);
        }
    }

    public void Setup(IServiceCollection sc)
    {
        SetupServices(sc, _services);

        SetupOptions(sc, _options);

        SetupMiddleware(sc, _middleware);

        var mwPipeline = BuildMiddlewarePipeline(_middleware);

        sc.TryAddScoped(sp => StoreFactory.Create(_initialState, mwPipeline(sp)));
        sc.TryAddScoped<IStore<TState>>(sp => sp.GetRequiredService<IInitializableStore<TState>>());
        sc.AddScoped<IStore>(sp => sp.GetRequiredService<IInitializableStore<TState>>());
        sc.AddScoped<IInitializableStore>(sp => sp.GetRequiredService<IInitializableStore<TState>>());
    }

    public IInitializableStore<TState> Build(IServiceProvider sp)
    {
        var store = sp.GetRequiredService<IInitializableStore<TState>>();
        foreach (var oncreate in _onStoreCreation)
        {
            oncreate(sp, store);
        }
        return store;
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
