namespace Rewind.Server.Store
{
    public class ServerUserStore
    {
        //Id
        public Guid Id { get; private set; }
        
        //FK
        public Guid BranchId { get; private set; }

        //Unique Immutable
        public string? OwnerId { get; private set; }

        //Links
        public ServerBranch? Branch { get; set; }


        public ServerUserStore() { }


        public ServerUserStore(Guid id, Guid branchId, string? ownerId, ServerBranch? branch = null)
        {
            Id = id;
            BranchId = branchId;
            OwnerId = ownerId;
        }

        public static ServerUserStore CreateUserStore(ServerState state, string ownerUserId)
        {
            return new ServerUserStore(Guid.NewGuid(), state.BranchId, ownerUserId, state.Branch);
        }
    }
}
