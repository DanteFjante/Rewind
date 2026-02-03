using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rewind.Common;
using Rewind.Middleware;

namespace Rewind.Store.Internal;

internal class StoreBuilder<TState> : IStoreBuilder<TState>
{
    readonly Dictionary<Type, MiddlewareRegistration> _middlewareSetup;
    readonly Dictionary<Type, OptionsRegistration> _optionsSetup;
    readonly Dictionary<Type, ServiceRegistration> _serviceSetup;
    readonly Dictionary<Type, BaseMiddleware<TState>> _getMiddleware;

    
    TState _initialState;

    public StoreBuilder(TState initialState)
    {
        _initialState = initialState;
        _middlewareSetup = new();
        _optionsSetup = new();
        _serviceSetup = new();
    }

    #region Middleware
    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TMiddleware> factory)
        where TMiddleware : BaseMiddleware<TState>
    {
        MiddlewareRegistration registration = new MiddlewareRegistration(typeof(TMiddleware), setup, factory);
        _middlewareSetup.TryAdd(typeof(TMiddleware), registration);

        return this;
    }

    public IStoreBuilder<TState> AddMiddleware<TMiddleware>(ServiceLifetime serviceLifetime) 
        where TMiddleware : BaseMiddleware<TState>
    {
        MiddlewareRegistration registration = new MiddlewareRegistration(
            typeof(TMiddleware),
            sc =>
            {
                ServiceDescriptor descriptor = new ServiceDescriptor(
                    typeof(TMiddleware),
                    typeof(TMiddleware),
                    serviceLifetime);
                sc.TryAdd(descriptor);
            },
            (sp, _) => sp.GetRequiredService<TMiddleware>());

        _middlewareSetup.Add(typeof(TMiddleware), registration);
        return this;
    }
    public IStoreBuilder<TState> AddMiddleware<TMiddleware>() 
        where TMiddleware : BaseMiddleware<TState>
    {
        return AddMiddleware<TMiddleware>(ServiceLifetime.Scoped);
    }

    private void SetupMiddleware(
        IServiceCollection sc, 
        IEnumerable<MiddlewareRegistration> registrations)
    {
        foreach (var factory in registrations)
        {
            factory.setup(sc);
        }
    }

    private List<BaseMiddleware<TState>> BuildMiddlewares(
        IServiceProvider sp,
        IEnumerable<MiddlewareRegistration> mw,
        IEnumerable<OptionsRegistration> opt,
        IEnumerable<ServiceRegistration> srv)
    {
        IStoreFactory<TState> sf = CreateStoreFactory(sp, mw, opt, srv);

        List<BaseMiddleware<TState>> middleware = new();
        foreach (var registration in mw)
        {
            middleware.Add(registration.factory(sp, sf));
        }

        return middleware;
    }

    private record class MiddlewareRegistration(
        Type middlewareType, 
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, BaseMiddleware<TState>> factory);
    #endregion

    #region Options
    public IStoreBuilder<TState> AddOptions<TOptions>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TOptions> factory) 
        where TOptions : class
    {
        Type type = typeof(TOptions);
        string sectionName = HelperMethods.StoreName<TOptions>();
        OptionsRegistration registration = new OptionsRegistration(
            type,
            sectionName,
            setup,
            factory
            );

        _optionsSetup.Add(type, registration);

        return this;
    }


    public IStoreBuilder<TState> AddOptions<TOptions>(
        Action<TOptions>? dynamicSettings = null) 
        where TOptions : class
    {
        string storeName = HelperMethods.StoreName<TState>();
        string sectName = HelperMethods.StoreName<TOptions>();

        Console.WriteLine("SectName: " + sectName + ":" + storeName);

        OptionsRegistration setup = new OptionsRegistration(typeof(TOptions), storeName, sc =>
        {
            sc.AddOptions<TOptions>(storeName)
                .Configure<IConfiguration>((opt, config) =>
                {
                    if (!string.IsNullOrWhiteSpace(sectName))
                    {
                        var section = config.GetSection(sectName);
                        var sectionPerState = config.GetSection($"{sectName}:{storeName}");

                        if (section.Exists())
                            section.Bind(opt);

                        if (sectionPerState.Exists())
                            sectionPerState.Bind(opt);
                    }

                    dynamicSettings?.Invoke(opt);
                });

        },
        (sp, _) => {
            var options = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
            return options.Get(storeName);
        }
        );

        _optionsSetup.Add(typeof(TOptions), setup);

        return this;
    }

    private void SetupOptions(IServiceCollection sc, IEnumerable<OptionsRegistration> options)
    {
        foreach (var option in options)
        {
            option.Setup(sc);
        }
    }

    private record class OptionsRegistration
    (
        Type OptionsType,
        string SectionName,
        Action<IServiceCollection> Setup,
        Func<IServiceProvider, IStoreFactory<TState>, object> factory
    );

    #endregion

    #region Services
    public IStoreBuilder<TState> AddService<TService, TImpl>()
        where TService : class
        where TImpl : class, TService
    {
        var type = typeof(TService);
        ServiceRegistration registration = new ServiceRegistration(
            type,
            sc => sc.TryAddScoped<TService, TImpl>(),
            (sp, _) => sp.GetRequiredService<TService>()
            );
        _serviceSetup.Add(type, registration);
        return this;
    }

    public IStoreBuilder<TState> AddService<TService, TImpl>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TService> factory)
        where TService : class
        where TImpl : class, TService
    {
        var type = typeof(TService);
        ServiceRegistration registration = new ServiceRegistration(
            type,
            setup,
            factory
            );
        _serviceSetup.Add(type, registration);
        return this;
    }
    public IStoreBuilder<TState> AddService<TService>(
        Action<IServiceCollection> setup, 
        Func<IServiceProvider, IStoreFactory<TState>, TService> factory)
        where TService : class
    {
        var type = typeof(TService);
        ServiceRegistration registration = new ServiceRegistration(type, setup, factory);

        _serviceSetup.Add(typeof(TService), registration);
        
        return this;
    }
    public IStoreBuilder<TState> AddService<TService>(ServiceLifetime lifetime) 
        where TService : class
    => this.AddService<TService, TService>(lifetime);
    public IStoreBuilder<TState> AddService<TService, TImpl>(ServiceLifetime lifetime) 
        where TService : class
        where TImpl : class, TService
    {
        var servType = typeof(TService);
        var implType = typeof(TImpl);
        var desc = new ServiceDescriptor(servType, implType, lifetime);

        ServiceRegistration registration = new ServiceRegistration(
            servType,
            sc => sc.TryAdd(desc),
            (sp, sf) => sp.GetRequiredService<TService>()
            );
        _serviceSetup.Add(servType, registration);

        return this;
    }

    public IStoreBuilder<TState> AddService<TService>() where TService : class
    {
        ServiceRegistration registration = new ServiceRegistration(
            typeof(TService),
            sc => sc.TryAddScoped<TService>(),
            (sp, _) => sp.GetRequiredService<TService>()
            );

        _serviceSetup.Add(typeof(TService), registration);
        return this;
    }

    private void SetupServices(IServiceCollection sc, IEnumerable<ServiceRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            registration.setup(sc);
        }
    }

    private record class ServiceRegistration(
        Type ServiceType,
        Action<IServiceCollection> setup,
        Func<IServiceProvider, IStoreFactory<TState>, object> factory
        );
    #endregion

    #region Build
    public Func<IServiceProvider, Store<TState>> Build(IServiceCollection sc)
    {
        Console.WriteLine("Setting up services");
        SetupServices(sc, _serviceSetup.Values);

        Console.WriteLine("Setting up options");
        SetupOptions(sc, _optionsSetup.Values);

        Console.WriteLine("Setting up middleware: " + typeof(TState).Name);
        SetupMiddleware(sc, _middlewareSetup.Values);


        Console.WriteLine("Building store for: " + typeof(TState).Name);
        return sp => new Store<TState>(
            _initialState, 
            BuildMiddlewares(
                sp, 
                _middlewareSetup.Values, 
                _optionsSetup.Values, 
                _serviceSetup.Values));
    }

    private IStoreFactory<TState> CreateStoreFactory(
        IServiceProvider sp,
        IEnumerable<MiddlewareRegistration> mw,
        IEnumerable<OptionsRegistration> opt,
        IEnumerable<ServiceRegistration> srv)
    {
        StoreFactory<TState> sf = new StoreFactory<TState>();

        Dictionary<Type, Func<BaseMiddleware<TState>>> mwFuncs = new();
        foreach (var entry in mw)
        {
            mwFuncs.Add(entry.middlewareType, () => entry.factory(sp, sf));
        }
        Dictionary<Type, Func<object>> optFuncs = new();
        foreach (var entry in opt)
        {
            optFuncs.Add(entry.OptionsType, () => entry.factory(sp, sf));
        }

        Dictionary<Type, Func<object>> srvFuncs = new();
        foreach (var entry in srv)
        {
            srvFuncs.Add(entry.ServiceType, () => entry.factory(sp, sf));
        }

        sf.Setup(mwFuncs, optFuncs, srvFuncs);
        return sf;
    }

    #endregion


}
