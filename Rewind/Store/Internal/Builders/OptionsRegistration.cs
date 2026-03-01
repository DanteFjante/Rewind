using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Store.Builders
{
    internal class OptionsRegistration
    {
        public Action<IServiceCollection>? ProviderRegistration { get; }
        public Func<IServiceProvider, object> Factory { get; }

        public Type ServiceType;
        public OptionsRegistration(Action<IServiceCollection>? providerRegistration, Type serviceType)
        {
            ProviderRegistration = providerRegistration;
            ServiceType = serviceType;
        }
    }
}
