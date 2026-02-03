namespace Rewind.Store.Internal
{
    internal class StoreInitializer : IStoreInitializer
    {
        private readonly IEnumerable<IInitializableStore> stores;

        private readonly Dictionary<Type, IEnumerable<IStore>> _storeCaches;   
        public StoreInitializer(IEnumerable<IInitializableStore> stores)
        {
            this.stores = stores;
            this._storeCaches = new();
        }
        
        public async Task InitializeStoresAsync(CancellationToken ct = default)
        {
            foreach (var store in stores)
            {
                if (!store.IsInitialized)
                    await store.InitializeAsync(ct);
            }
        }

        public async Task<IEnumerable<IStore>> InitializeFromParentProperties<TParent>()
        {
            if (_storeCaches.TryGetValue(typeof(TParent), out var list))
            {
                return list;
            }
            else
            {
                Type parent = typeof(TParent);
                var types = GetIStorePropertyTypesFromParent<TParent>();

                List<IStore> istores = new List<IStore>();

                foreach (var store in stores)
                {
                    if (store.GetType().DeclaringType?.GenericTypeArguments.Any(x => types.Contains(x)) ?? false)
                    {
                        if (store is IStore)
                        {
                            await store.InitializeAsync();
                            istores.Add((IStore) store);

                        }
                    }
                }
                _storeCaches.Add(parent, istores);
                return istores;
            }
        }

        private IEnumerable<Type> GetIStorePropertyTypesFromParent<TParent>()
        {
             return typeof(TParent)
                .GetProperties()
                .Where(x
                    => x.PropertyType.IsGenericType
                    && x.PropertyType.GetGenericTypeDefinition().Equals(typeof(IStore<>)))
                .Select(x => x.PropertyType.GetGenericArguments().First());
        }

    }
}
