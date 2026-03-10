using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Store.Internal.Registrations
{
    internal class MiddlewareRegistration
    {
        public Action<IServiceCollection>? ProviderRegistration { get; }
        public Func<IServiceProvider, object> Factory { get; }

        public Type ServiceType;

        public MiddlewareRegistration(Action<IServiceCollection>? providerRegistration, Func<IServiceProvider, object> factory, Type serviceType)
        {
            ProviderRegistration = providerRegistration;
            Factory = factory;
            ServiceType = serviceType;
        }
    }
}
