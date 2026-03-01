using Rewind.Server.Base.Store;

namespace Rewind.Extensions.Persistence.Server
{
    public interface IServerStorageService : IPersistanceService
    {
        public ValueTask<bool> RegisterStore(bool isSharedStore, PersistenceKey key);

        public ValueTask<bool> JoinBranch(string branchName);

        public ValueTask<HashSet<string>> GetBranches(PersistenceKey key);
    }
}
