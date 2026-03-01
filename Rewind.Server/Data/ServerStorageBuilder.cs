using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Server.Data
{
    public class ServerStorageBuilder
    {
        internal Action<IServiceProvider, DbContextOptionsBuilder> configureDBContext;
        internal bool UseDBContext;
        internal Action<MemoryCacheOptions> configureMemoryCache;
        internal bool UseMemoryCache;
        public ServerStorageBuilder AddDBContext(Action<IServiceProvider, DbContextOptionsBuilder> dbContextSetup)
        {
            if (UseMemoryCache)
            {
                configureDBContext = (sp, c) =>
                {
                    var memoryCache = sp.GetService<IMemoryCache>();
                    if (memoryCache != null)
                        c.UseMemoryCache(memoryCache);

                    dbContextSetup(sp, c);
                };
            }
            else
            {
                configureDBContext = dbContextSetup;

            }
            UseDBContext = true;

            return this;
        }

        public ServerStorageBuilder AddMemoryCache(Action<MemoryCacheOptions>? cacheSetup = null)
        {
            configureMemoryCache = cacheSetup ?? ((_) => { });
            if (UseDBContext)
            {
                var setup = configureDBContext;
                configureDBContext = (sp, c) =>
                {
                    var memoryCache = sp.GetService<IMemoryCache>();
                    if (memoryCache != null)
                        c.UseMemoryCache(memoryCache);

                    setup(sp, c);
                };
            }
            UseMemoryCache = true;

            return this;
        }

    }
}
