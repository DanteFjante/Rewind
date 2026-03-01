using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Extensions.Store;
using Rewind.Store;

namespace Rewind.Server.Builders
{
    public class ServerBuilder
    {
        List<ServiceRegistration> services { get; set; } = new();
        List<OptionsRegistration> options { get; set; } = new();

        public ServerBuilder()
        {

        }

        public ServerBuilder AddService<TService>(Func<ServiceBuilder<TService>, ServiceBuilder<TService>>? builder = null)
            where TService : class
        {
            ServiceBuilder<TService> sb = new ServiceBuilder<TService>();
            if(builder != null)
                sb = builder(sb);

            services.Add(sb.Build());
            return this;
        }

        public ServerBuilder AddOptions<TOptions>(Func<OptionsBuilder<TOptions>, OptionsBuilder<TOptions>>? builder = null)
            where TOptions : class
        {
            OptionsBuilder<TOptions> ob = new OptionsBuilder<TOptions>();
            if(builder != null)
                ob = builder(ob);

            options.Add(ob.Build());
            return this;
        }

        public void Build(IServiceCollection sc)
        {
            foreach (var service in services)
            {
                if(service.ProviderRegistration != null)
                    service.ProviderRegistration(sc);
            }

            foreach (var option in options)
            {
                if (option.ProviderRegistration != null)
                {
                    option.ProviderRegistration(sc);
                }
            }

            sc.TryAddScoped<IStateManager, ExtendedStateManager>();
            sc.TryAddScoped<IStateProvider>(sp => sp.GetRequiredService<IStateManager>());
        }

    }
}
