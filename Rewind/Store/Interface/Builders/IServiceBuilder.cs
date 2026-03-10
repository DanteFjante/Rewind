using Microsoft.Extensions.DependencyInjection;
using Rewind.Store.Internal.Registrations;

namespace Rewind.Store.Builders
{
    public interface IServiceBuilder<TState, TService>
        where TService : class
    {
        public IServiceBuilder<TState, TService> UseDefaultSetup();

        public IServiceBuilder<TState, TService> SetImplementationType<TImpl>() where TImpl : class, TService;

        public IServiceBuilder<TState, TService> SetFactory(Action<IServiceCollection> provider, Func<IServiceProvider, TService> factory);
        public IServiceBuilder<TState, TService> SetFactory(Func<IServiceProvider, TService> factory);

        public IServiceBuilder<TState, TService> SetLifetime(ServiceLifetime lifetime);

        public IServiceBuilder<TState, TService> SetServiceKey(object? serviceKey);
        internal ServiceRegistration Build();
    }
}
