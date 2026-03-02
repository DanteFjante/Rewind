using Microsoft.Extensions.Caching.Memory;
using Rewind.Extensions.Persistence;
using Rewind.Server.Users;

namespace Rewind.Server.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private record CacheKey(string UserId, string Type, string Name);

        private CacheKey GetKey(PersistenceKey key) => new CacheKey(User.UserId, key.Name, key.Type);

        public IMemoryCache Cache;
        public CacheSettings Settings;
        public IUserContext? User;
        public MemoryCacheService(IMemoryCache cache, CacheSettings settings, IUserContext? user = null)
        {
            Cache = cache;
            Settings = settings;
            User = user;
        }

        public ValueTask Remove(PersistenceKey key)
        {

            Cache.Remove(key);

            return ValueTask.CompletedTask;
        }
        public ValueTask Set(PersistenceKey key, PersistenceData data)
        {
            Cache.Set(key, data, Settings.StateLifetime);

            return ValueTask.CompletedTask;
        }
        public ValueTask<PersistenceData?> Get(PersistenceKey key)
        {
            var data = Cache.Get<PersistenceData>(key);

            return ValueTask.FromResult(data);
        }
    }
}
