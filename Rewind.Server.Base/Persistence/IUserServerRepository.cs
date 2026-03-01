using Rewind.Extensions.Persistence;
using Rewind.Server.Base.Store;

namespace Rewind.Server.Base.Persistence
{
    public interface IUserServerRepository : IServerRepository
    {
        public ValueTask<ServerUserStore?> GetUserStore(PersistenceKey key, bool includeLinks = false);
        public ValueTask<HashSet<ServerUserStore>> GetUserStoresOfType(string type, bool includeLinks = false);

        public ValueTask<bool> JoinBranch(string branchName);
        public ValueTask<bool> RemoveUserStore(PersistenceKey key);
    }
}
