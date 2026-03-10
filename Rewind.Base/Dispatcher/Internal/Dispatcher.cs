using Rewind.Commands;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Store;
using Rewind.Store.Internal.Builders;
using Rewind.Effects;
namespace Rewind.Base.Dispatcher.Internal
{
    public class Dispatcher : IDispatcher
    {
        public IStoreProvider StoreProvider { get; }
        public IReducerManager ReducerManager { get; }
        public Dictionary<Type, List<IReducerExecutor>> Reducers { get; }
        public IEffectProvider Effects { get; }


        public Dispatcher(IStoreProvider storeProvider, IReducerManager reducerManager, IEnumerable<IReducerExecutor> reducerExecutors, IEffectProvider effects)
        {
            StoreProvider = storeProvider;
            ReducerManager = reducerManager;

            Effects = effects;

            Reducers = new();
            foreach (var executor in reducerExecutors)
            {
                if (Reducers.ContainsKey(executor.CommandType))
                {
                    Reducers[executor.CommandType].Add(executor);
                }
                else
                {
                    Reducers.Add(executor.CommandType, [executor]);
                }
            }
        }

        public async ValueTask AddReducer<TState, TCommand>(IReducer<TState, TCommand> reducer, CancellationToken ct = default)
            where TCommand : ICommand
        {
            var store = await StoreProvider.GetStore<TState>();
            
            if (store == null)
                return;

            if (store[reducer.StoreKey.Name] == null)
            {
                await store.CreateStateAsync(reducer.StoreKey.Name);
                await AddReducer(new Reducer<TState, UpdateState<TState>>(command => command.Reducer, reducer.StoreKey.Name));
            }

            var executor = new ReducerExecutor<TState, TCommand>(reducer, store);
            if (Reducers.TryGetValue(reducer.CommandType, out var list))
            {
                list.Add(executor);
            }
            else
            {
                Reducers[reducer.CommandType] = [executor]; 
            }

            ReducerManager.AddReducer(reducer);
        }


        public async ValueTask DispatchAsync<TCommand>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand
        {
            if (Reducers.TryGetValue(typeof(TCommand), out var reducers))
            {
                foreach (var r in reducers)
                {
                    await r.ExecuteAsync(command);
                }
            }

            foreach (var effect in Effects.GetEffects<TCommand>())
            {
                await effect.HandleAsync(command);
            }
        }
    }
}
