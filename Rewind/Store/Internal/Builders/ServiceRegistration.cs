using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Store.Builders
{
    internal class ServiceRegistration
    {
        public Action<IServiceCollection>? ProviderRegistration { get; }
        public Func<IServiceProvider, object> Factory { get; }
        public Type ServiceType;
        public ServiceRegistration(Action<IServiceCollection>? providerRegistration, Func<IServiceProvider, object> factory, Type serviceType)
        {
            ProviderRegistration = providerRegistration;
            Factory = factory;
            ServiceType = serviceType;
        }
    }
}
