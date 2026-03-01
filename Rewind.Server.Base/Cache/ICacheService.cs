using Rewind.Extensions.Persistence;

namespace Rewind.Server.Cache
{
    public interface ICacheService
    {
        public ValueTask Remove(PersistenceKey key);
        public ValueTask Set(PersistenceKey key, PersistenceData data);
        public ValueTask<PersistenceData?> Get(PersistenceKey key);

    }
}
