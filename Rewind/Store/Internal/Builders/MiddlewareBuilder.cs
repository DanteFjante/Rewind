using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Middleware;
namespace Rewind.Store.Builders
{
    internal class MiddlewareBuilder<TState, TMiddleware> : IMiddlewareBuilder<TState, TMiddleware>
        where TMiddleware : BaseMiddleware<TState>
    {
        public Func<IServiceProvider, TMiddleware>? Factory { get; set; }

        public Action<IServiceCollection>? Setup { get; set; }

        public Dictionary<string, object> Settings { get; }
        
        public Type ServiceType => typeof(BaseMiddleware<TState>);
        public Type ImplementationType => typeof(TMiddleware);
        public ServiceLifetime ServiceLifetime => Settings.TryGetValue("Lifetime", out var key) ? (ServiceLifetime) key : ServiceLifetime.Scoped;


        public MiddlewareBuilder()
        {
            Settings = new();
            UseDefaultSetup();
        }

        public IMiddlewareBuilder<TState, TMiddleware> SetFactory(Func<IServiceProvider, TMiddleware> factory)
        {
            Factory = factory;
            Setup = null;
            return this;
        }

        public IMiddlewareBuilder<TState, TMiddleware> SetFactory(Action<IServiceCollection> provider, Func<IServiceProvider, TMiddleware> factory)
        {
            Factory = factory;
            Setup = Setup;
            return this;
        }

        public IMiddlewareBuilder<TState, TMiddleware> UseDefaultSetup()
        {
            Factory = sp =>
            {
                var mw = sp.GetRequiredService<TMiddleware>();
                return mw;
            };

            Setup = (sc) =>
            {
                ServiceDescriptor descriptor = new ServiceDescriptor(ServiceType, sp => sp.GetRequiredService<TMiddleware>(), ServiceLifetime);
                ServiceDescriptor impldescriptor = new ServiceDescriptor(ImplementationType, ImplementationType, ServiceLifetime);
                sc.TryAdd(descriptor);
                sc.TryAdd(impldescriptor);
            };

            return this;
        }

        public IMiddlewareBuilder<TState, TMiddleware> SetLifeTime(ServiceLifetime lifetime)
        {
            Settings["Lifetime"] = lifetime;
            return this;
        }

        public MiddlewareRegistration Build()
        {
            MiddlewareRegistration registration = new MiddlewareRegistration(Setup, Factory!, ServiceType);
            return registration;
        }
    }
}
