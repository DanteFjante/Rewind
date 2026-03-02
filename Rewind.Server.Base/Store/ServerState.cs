using Rewind.Extensions.Persistence;

namespace Rewind.Server.Store
{
    public record class ServerState
    {
        //Id
        public Guid Id { get; private set; }
        
        //FK
        public Guid BranchId { get; private set; }

        //Immutable
        public string? AuthorUserId { get; private set; }
        public long Version { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string StateJson { get; private set; }
        public string? Reason { get; set; }

        //From Store
        public string StoreType { get; private set; }
        public string StoreName { get; private set; }
        public bool IsPublic { get; set; }

        //Links
        public ServerBranch? Branch { get; private set; }


#pragma warning disable CS8618 //For entityframework
        public ServerState() { }
#pragma warning restore CS8618

        public ServerState(Guid id, Guid branchId, string? authorUserId, long version, DateTime updatedAt, string? reason, string stateJson, ServerBranch branch)
        {
            this.Id = id;
            this.BranchId = branchId;


            this.AuthorUserId = authorUserId;

            this.Version = version;
            this.UpdatedAt = updatedAt;
            this.Reason = reason;
            this.StateJson = stateJson;

            this.StoreType = branch.StoreType;
            this.StoreName = branch.StoreName;
            this.IsPublic = branch.IsPublic;
        }

        public ServerState(Guid id, Guid branchId, string? authorUserId, long version, DateTime updatedAt, string? reason, string stateJson, string storeType, string storeName, bool isPublic, ServerBranch? branch = null)
        {
            this.Id = id;
            this.BranchId = branchId;

            this.AuthorUserId = authorUserId;

            this.Version = version;
            this.UpdatedAt = updatedAt;
            this.Reason = reason;
            this.StateJson = stateJson;

            this.StoreType = storeType;
            this.StoreName = storeName;
            this.IsPublic = isPublic;

            this.Branch = branch;
        }
        public ServerState Update(PersistenceData data)
        {
            return this with
            {
                Id = Guid.NewGuid(),
                UpdatedAt = data.UpdatedAt,
                Version = data.Version,
                Reason = data.Reason,
                StateJson = data.StateJson
            };

        }

        public PersistenceData ToPersistenceData()
        {
            return new PersistenceData(StoreType, StoreName, Version, StateJson, UpdatedAt, Reason);
        }

        public static ServerState? CreateStoreWithState(string? userId, string branchName, bool isPublic, PersistenceData data)
        {
            Guid storeId = Guid.NewGuid();

            ServerStore store = new ServerStore(storeId, data.Type, data.Name, isPublic);

            return CreateStateWithBranch(store, branchName, userId, data);
        }

        public static ServerState? CreateStateWithBranch(ServerStore store, string branchName, string? userId,  PersistenceData data)
        {
            Guid branchId = Guid.NewGuid();

            Guid stateId = Guid.NewGuid();

            ServerBranch branch = new ServerBranch(branchId, branchName, data.Version, stateId, data.Version, stateId, store);

            ServerState? state = CreateState(stateId, store, branch, userId, data);

            return state;
        }

        public static ServerState? CreateState(ServerStore store, ServerBranch branch, string? userId, PersistenceData data)
        {
            Guid stateId = Guid.NewGuid();

            return CreateState(stateId, store, branch, userId, data);
        }

        private static ServerState? CreateState(Guid id, ServerStore store, ServerBranch branch, string? userId, PersistenceData data)
        {
            Guid branchId = Guid.NewGuid();

            ServerState state = new ServerState(id, branchId, userId, data.Version, data.UpdatedAt, data.Reason, data.StateJson, store.StoreType, store.StoreName, store.IsPublic, branch);

            return state;
        }


    }
}
