using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rewind.Server.Builders
{
    public class ServiceBuilder<TService>
        where TService : class
    {
        public Action<IServiceCollection>? Provider { get; set; }
        public Func<IServiceProvider, TService>? Factory { get; set; }
        public Dictionary<string, object> Settings { get; }

        public Type ServiceType => typeof(TService);
        public Type ImplementationType
            => Settings.TryGetValue("ImplType", out var type)
            ? (Type)type
            : typeof(TService);
        public object? ServiceKey
            => Settings.TryGetValue("Key", out var key)
            ? key
            : null;
        public ServiceLifetime ServiceLifetime
            => Settings.TryGetValue("Lifetime", out var lifetime)
            ? (ServiceLifetime)lifetime
            : ServiceLifetime.Scoped;

        public ServiceBuilder()
        {
            Settings = new();

            UseDefaultSetup();
        }

        public ServiceBuilder<TService> SetFactory(Func<IServiceProvider, TService> factory)
        {
            Provider = sc => sc.TryAddScoped(factory);
            Factory = factory;

            return this;
        }

        public ServiceBuilder<TService> SetFactory(
            Action<IServiceCollection> provider,
            Func<IServiceProvider, TService> factory)
        {
            Factory = factory;
            Provider = provider;

            return this;
        }

        public ServiceBuilder<TService> SetImplementationType<TImpl>() where TImpl : class, TService
        {
            Settings["ImplType"] = typeof(TImpl);
            if (ServiceKey != null)
            {
                Factory = (sp) => sp.GetRequiredKeyedService<TService>(ServiceKey);

                Provider = (sc) =>
                {
                    ServiceDescriptor desc = new ServiceDescriptor(ServiceType, ServiceKey, typeof(TImpl), ServiceLifetime);
                    sc.TryAdd(desc);
                };
            }
            else
            {
                Factory = (sp) => sp.GetRequiredService<TService>();

                Provider = (sc) =>
                {
                    ServiceDescriptor desc = new ServiceDescriptor(ServiceType, typeof(TImpl), ServiceLifetime);
                    sc.TryAdd(desc);
                };
            }
            return this;
        }

        public ServiceBuilder<TService> UseDefaultSetup()
        {
            Factory = sp => sp.GetRequiredService<TService>()!;
            Provider = (sc) =>
            {
                if (ServiceKey != null)
                {
                    ServiceDescriptor descriptor = new ServiceDescriptor(ServiceType, ServiceKey, ImplementationType, ServiceLifetime);
                    sc.TryAdd(descriptor);
                }
                else
                {
                    ServiceDescriptor descriptor = new ServiceDescriptor(ServiceType, ImplementationType, ServiceLifetime);
                    sc.TryAdd(descriptor);
                }
            };
            return this;
        }

        public ServiceBuilder<TService> SetLifetime(ServiceLifetime lifetime)
        {
            Settings["Lifetime"] = lifetime;
            return this;
        }

        public ServiceBuilder<TService> SetServiceKey(object? serviceKey)
        {
            if (serviceKey != null)
                Settings["Key"] = serviceKey;
            else
                Settings.Remove("Key");
            return this;
        }

        internal ServiceRegistration Build()
        {

            ServiceRegistration registration = new ServiceRegistration(Provider, Factory!, ServiceType);

            return registration;
        }
    }
}
