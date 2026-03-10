using Rewind.Commands;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Store;
using Rewind.Store.Internal.Builders;
namespace Rewind.Base.Dispatcher.Internal
{
    public class Dispatcher : IDispatcher
    {
        public IStoreProvider StoreProvider { get; }
        public IReducerManager ReducerManager { get; }
        public Dictionary<Type, List<IReducerExecutor>> Reducers { get; }
        public IEffectRepository Effects { get; }


        public Dispatcher(IStoreProvider storeProvider, IReducerManager reducerManager, IEnumerable<IReducerExecutor> reducerExecutors, IEffectRepository effects)
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

        public async ValueTask<bool> AddReducer<TState, TCommand>(IReducer<TState, TCommand> reducer, CancellationToken ct = default)
            where TCommand : ICommand
        {
            var store = await StoreProvider.GetStore<TState>();
            
            if (store == null)
                return false;

            if (store[reducer.StoreKey.Name] == null)
            {
                return false;
            }
            AddReducer(reducer, store);
            return true;
        }


        private void AddReducer<TState, TCommand>(IReducer<TState, TCommand> reducer, IStore<TState> store)
            where TCommand : ICommand
        {
            var executor = new ReducerExecutor<TState, TCommand>(reducer, store, reducer.CommandFilter);
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
                var rs = reducers.Where(x => x.CommandFilter?.Invoke(command.CommandName) ?? true);
                foreach (var r in rs)
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
