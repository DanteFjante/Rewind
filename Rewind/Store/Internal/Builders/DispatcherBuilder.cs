using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Base.Dispatcher.Internal;
using Rewind.Base.Store.Interface;
using Rewind.Commands;
using Rewind.Effects;
using Rewind.Extensions.Store;
using Rewind.Store.Builders;
using Rewind.Store.Internal.Registrations;
using System.Data;

namespace Rewind.Store.Internal.Builders
{
    internal class DispatcherBuilder : IDispatcherBuilder
    {

        public List<StoreRegistration> Stores { get; set; }
        public List<EffectRegistration> Effects { get; set; }
        public List<ReducerRegistration> Reducers { get; set; }

        Action<IServiceCollection> StateManagerSetup { get; set; }
        Action<IServiceCollection> StoreManagerSetup { get; set; }

        public DispatcherBuilder()
        {
            Stores = new();
            Effects = new();
            Reducers = new();

            StoreManagerSetup = sc => sc.TryAddScoped<IStoreManager, StoreManager>();

            StateManagerSetup = sc => sc.TryAddScoped<IStateManager, StateManager>();
        }

        public IDispatcherBuilder SetStoreManager(Func<IServiceProvider, IStoreManager> storeManagerFactory)
        {
            StoreManagerSetup = sc => sc.TryAddScoped(storeManagerFactory);
            return this;
        }
        
        public IDispatcherBuilder SetStoreManager<StoreManager>() where StoreManager : class, IStoreManager
        {
            StoreManagerSetup = sc => sc.TryAddScoped<IStoreManager, StoreManager>();
            return this;
        }

        public IDispatcherBuilder SetStateManager(Func<IServiceProvider, IStateManager> stateManagerFactory)
        {
            StateManagerSetup = sc => sc.TryAddScoped(stateManagerFactory);
            return this;
        }
        
        public IDispatcherBuilder SetStateManager<StateManager>() where StateManager : class, IStateManager
        {
            StateManagerSetup = sc => sc.TryAddScoped<IStateManager, StateManager>();
            return this;
        }


        public IDispatcherBuilder RegisterEffect<TEffect, TCommand>() 
            where TEffect : class, IEffect
            where TCommand : ICommand
        {
            Effects.Add(new EffectRegistration()
            {
                EffectSetup = sc =>
                {
                    sc.TryAddScoped<TEffect>();
                    sc.AddScoped<IEffect>(sp => sp.GetRequiredService<TEffect>());
                },
                Factory = sp => sp.GetRequiredService<TEffect>(),
                EffectType = typeof(TEffect),
                CommandType = typeof(TCommand)
            });
            return this;
        }

        public IDispatcherBuilder RegisterEffect<TEffect, TCommand>(Func<IServiceProvider, TEffect> factory)
            where TEffect : class, IEffect
            where TCommand : ICommand
        {
            Effects.Add(new EffectRegistration()
            {
                EffectSetup = sc => {
                    sc.TryAddScoped(factory);
                    sc.AddScoped<IEffect>(sp => sp.GetRequiredService<TEffect>());
                },
                Factory = sp => sp.GetRequiredService<TEffect>(),
                EffectType = typeof(TEffect),
                CommandType = typeof(TCommand)
            });
            return this;
        }
        public IDispatcherBuilder RegisterReducer<TState, TCommand>(Reduction<TState, TCommand> reducer, string stateName = "", Predicate<string>? commandFilter = null)
            where TCommand : ICommand
        {
            var r = new Reducer<TState, TCommand>(reducer, stateName, commandFilter);

            return RegisterReducer(r);
        }

        public IDispatcherBuilder RegisterReducer<TState, TCommand>(IReducer<TState, TCommand> reducer) 
            where TCommand : ICommand
        {
            ReducerRegistration registration = new() 
            { 
                Reducer = reducer,
                ExecutorFactory = sp => new ReducerExecutor<TState, TCommand>(reducer, sp.GetRequiredService<IStore<TState>>(), reducer.CommandFilter)
            };

            Reducers.Add(registration);
            return this;
        }


        public IDispatcherBuilder RegisterStore<TState>(TState InitialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? builder = null)
            where TState : class
            => RegisterStore(() => InitialState, builder);
        public IDispatcherBuilder RegisterStore<TState>(Func<TState> InitialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? builder = null)
            where TState : class
        {
            StoreBuilder<TState> sb = new StoreBuilder<TState>(InitialState);

            if(builder != null)
                sb = (StoreBuilder<TState>) builder(sb);

            StoreRegistration sr = new StoreRegistration()
            {
                TState = typeof(TState),
                StoreSetup = sb.Setup,
                StoreFactory = sb.Build
            };

            RegisterReducer<TState, UpdateState<TState>>(command => command.Reducer, "", string.IsNullOrEmpty);
            RegisterEffect<CreateStateEffect<TState>, CreateState<TState>>();
            Stores.Add(sr);

            return this;
        }

        public Func<IServiceProvider, IDispatcher> BuildFactory(IServiceCollection sc)
        {
            sc.TryAddSingleton<IReducerManager>(sp => new ReducerManager(Reducers.Select(x => x.Reducer).ToList()));

            StoreManagerSetup(sc);
            sc.TryAddScoped(typeof(IStoreProvider), sp => sp.GetRequiredService<IStoreManager>());
            StateManagerSetup(sc);
            sc.TryAddScoped(typeof(IStateProvider), sp => sp.GetRequiredService<IStateManager>());


            List<EffectDescriptor> effects = new();
            foreach (var effect in Effects)
            {
                effect.EffectSetup(sc);
                effects.Add(new EffectDescriptor() 
                {
                    EffectType = effect.EffectType,
                    CommandType = effect.CommandType,
                    Factory = effect.Factory
                });
            }

            sc.TryAddScoped<IEffectRepository>(sp => new EffectRepository(sp, effects));

            foreach (var store in Stores)
            {
                store.StoreSetup(sc);
            }

            sc.TryAddScoped<IDispatcher>(sp 
                => new Dispatcher(
                    sp.GetRequiredService<IStoreProvider>(), 
                    sp.GetRequiredService<IReducerManager>(), 
                    Reducers.Select(x => x.ExecutorFactory(sp)), 
                    sp.GetRequiredService<IEffectRepository>()
                    ));

            return sp => sp.GetRequiredService<IDispatcher>();
        }

    }
}
