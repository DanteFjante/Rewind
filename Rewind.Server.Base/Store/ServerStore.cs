namespace Rewind.Server.Base.Store
{
    public record class ServerStore
    {
        //Id
        public Guid Id { get; private set; }
        //Unique immutable
        public string StoreType { get; private set; }
        public string StoreName { get; private set; }
        //Immutable
        public bool IsPublic { get; private set; }
        public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618 //Required for Entity Framework
        public ServerStore() { }
#pragma warning restore CS8618

        public ServerStore(Guid Id, string StoreType, string StoreName, bool IsPublic, DateTime? CreatedAt = null)
        {
            this.Id = Id;
            this.StoreType = StoreType;
            this.StoreName = StoreName;
            this.IsPublic = IsPublic;
            this.CreatedAt = CreatedAt ?? DateTime.UtcNow;
        }
    }
}
