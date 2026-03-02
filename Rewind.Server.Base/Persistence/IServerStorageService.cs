using Rewind.Extensions.Persistence;

namespace Rewind.Server.Persistence
{
    public interface IServerStorageService : IPersistanceService
    {
        public ValueTask<bool> RegisterStore(bool isSharedStore, PersistenceKey key);

        public ValueTask<bool> JoinBranch(string branchName);

        public ValueTask<HashSet<string>> GetBranches(PersistenceKey key);
    }
}
