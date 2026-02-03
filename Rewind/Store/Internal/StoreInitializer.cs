namespace Rewind.Store.Internal
{
    internal class StoreInitializer : IStoreInitializer
    {
        private readonly IEnumerable<IInitializableStore> stores;
        public StoreInitializer(IEnumerable<IInitializableStore> stores)
        {
            this.stores = stores;
        }

        public async Task InitializeStores(CancellationToken ct = default)
        {
            foreach (var store in stores)
            {
                if (!store.IsInitialized)
                    await store.InitializeAsync(ct);
            }
        }
    }
}
