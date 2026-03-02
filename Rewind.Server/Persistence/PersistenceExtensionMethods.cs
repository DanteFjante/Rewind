using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Extensions.Persistence;
using Rewind.Server.Builders;
using Rewind.Server.Cache;
using Rewind.Server.Data;

namespace Rewind.Server.Persistence
{
    public static class PersistenceExtensionMethods
    {

        public static ServerBuilder AddPersistence(this ServerBuilder serverBuilder, Func<ServerStorageBuilder, ServerStorageBuilder> configure)
        {
            ServerStorageBuilder builder = new ServerStorageBuilder();
            builder = configure(builder);

            return serverBuilder
                .AddOptions<PersistenceSettings>()
                .AddOptions<CacheSettings>()
                .AddService<IUserServerRepository>(x => x.SetFactory(
                    sc =>
                    {
                        sc.TryAddScoped<IUserServerRepository, ServerRepository>();
                    },
                    sp => sp.GetRequiredService<IUserServerRepository>()
                    ))
                .AddService<IServerRepository>(x => x.SetFactory(
                    sc =>
                    {
                        if (!sc.Any(x => x.ServiceType == typeof(IMemoryCache)) && builder.UseMemoryCache)
                        {

                            sc.AddMemoryCache(builder.configureMemoryCache);

                            sc.TryAddScoped<ICacheService, MemoryCacheService>();
                        }

                        if (!sc.Any(x => x.ServiceType == typeof(RewindDbContext) && builder.UseDBContext))
                        {
                            sc.AddDbContext<RewindDbContext>(builder.configureDBContext);
                        }
                        sc.TryAddScoped<IServerRepository>(sp => sp.GetRequiredService<IUserServerRepository>());
                    },
                    sp => sp.GetRequiredService<IServerRepository>()
                    ))
                .AddService<IServerStorageService>(b => b.SetFactory(
                    sc => {
                        sc.TryAddScoped<IServerStorageService, ServerStorageService>();
                    },
                    sp => sp.GetRequiredService<IServerStorageService>()
                    ))
                .AddService<IPersistanceService>(b => b.SetFactory(
                    sc => {
                        sc.TryAddScoped<IPersistanceService>(sp => sp.GetRequiredService<IServerStorageService>());
                    },
                    sp => sp.GetRequiredService<IPersistanceService>()
                    ));
        }
    }
}
