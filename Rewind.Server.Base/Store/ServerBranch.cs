namespace Rewind.Server.Base.Store
{
    public class ServerBranch
    {
        //Id
        public Guid Id { get; set; }
        //FK
        public Guid StoreId { get; set; }

        //Immutable
        public string Name { get; set; }
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        public bool IsPublic { get; set; }

        //Mutable
        public long LastVersion { get; set; }
        public Guid LastStateId { get; set; }

        public long OldestVersion { get; set; }
        public Guid OldestStateId { get; set; }

        //Link
        public HashSet<ServerState>? States { get; set; }
        public ServerStore? Store { get; set; }
        public ServerState? LastState => States?.FirstOrDefault(x => x.Version == LastVersion);

        #pragma warning disable CS8618 //For entity framework to work
        public ServerBranch() { }
        #pragma warning restore CS8618

        public ServerBranch(
            Guid id, 
            Guid storeId, 
            string name,
            string storeType, 
            string storeName, 
            bool isPublic, 
            long lastVersion, 
            Guid lastStateId, 
            long oldestVersion, 
            Guid olderstStateId, 
            ServerStore? store = null, 
            IEnumerable<ServerState>? states = null)
        {
            StoreId = storeId;

            Name = name;
            StoreName = storeName;
            StoreType = storeType;
            IsPublic = isPublic;

            LastVersion = lastVersion;
            LastStateId = lastStateId;
            OldestVersion = oldestVersion;
            OldestStateId = olderstStateId;

            Store = store;
            States = states?.ToHashSet();
        }

        public ServerBranch(
            Guid id, 
            string name,
            long lastVersion, 
            Guid lastStateId, 
            long oldestVersion, 
            Guid olderstStateId, 
            ServerStore store, 
            IEnumerable<ServerState>? states = null)
        {
            StoreId = store.Id;

            Name = name;
            StoreName = store.StoreName;
            StoreType = store.StoreType;
            IsPublic = store.IsPublic;

            LastVersion = lastVersion;
            LastStateId = lastStateId;
            OldestVersion = oldestVersion;
            OldestStateId = olderstStateId;

            Store = store;
            States = states?.ToHashSet();
        }

        public void SetLastState(ServerState state)
        {
            LastStateId = state.Id;
            LastVersion = state.Version;
        }

        public void SetOldestState(ServerState state)
        {
            OldestStateId = state.Id;
            OldestVersion = state.Version;
        }
    }
}
