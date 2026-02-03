using Rewind.LocalStorage;
using Rewind.Redux.Store.Interface;
using Rewind.Redux.Store.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rewind.Redux.Store
{
    public static class StoreExtensionMethods
    {

        public static void AddStore<TState>(this IServiceCollection services, TState initialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? factory = null)
        {
            services.TryAddScoped<IStoreInitializer, StoreInitializer>();
            
            StoreBuilder<TState> storeBuilder = new StoreBuilder<TState>(initialState);

            if(factory != null)
                factory(storeBuilder);

            var storeFactory = storeBuilder.Build(services);

            services.TryAddScoped(sp => storeFactory(sp));
            services.TryAddScoped<IStore<TState>>(sp => sp.GetRequiredService<Store<TState>>());
            services.AddScoped<IInitializableStore>(sp => sp.GetRequiredService<Store<TState>>());
            services.AddScoped<IInitializableStore<TState>>(sp => sp.GetRequiredService<Store<TState>>());
        }
    }
}
