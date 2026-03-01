using Microsoft.Extensions.DependencyInjection;
using Rewind.Store.Builders;

namespace Rewind.Store
{
    public static class StoreExtensionMethods
    {

        public static IServiceCollection AddStore<TState>(this IServiceCollection services, TState initialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? factory = null)
        {
            
            StoreBuilder<TState> storeBuilder = new StoreBuilder<TState>(initialState);

            Func<IStoreBuilder<TState>, IStoreBuilder<TState>> internalFactory = (sb) => (factory?.Invoke(sb) ?? sb);

            storeBuilder = (StoreBuilder<TState>) internalFactory(storeBuilder);

            storeBuilder.Build(services);

            return services;
        }
    }
}
