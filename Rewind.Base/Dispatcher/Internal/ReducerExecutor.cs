using Rewind.Commands;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Store;

namespace Rewind.Base.Dispatcher.Internal
{
    public sealed class ReducerExecutor<TState, TCommand> : IReducerExecutor 
        where TCommand : ICommand
    {
        private IReducer<TState, TCommand> reducer;
        private IStore<TState> store;
        public Type StateType { get; } = typeof(TState);
        public Type CommandType { get; } = typeof(TCommand);
        public IReducer Reducer => reducer;

        public Predicate<string>? CommandFilter { get; }

        public ReducerExecutor(IReducer<TState, TCommand> reducer, IStore<TState> store, Predicate<string>? commandFilter = null)
        {
            this.reducer = reducer;
            this.store = store;
            CommandFilter = commandFilter;
        }

        public ValueTask ExecuteAsync(TCommand command, CancellationToken ct = default)
        {

                return store.UpdateAsync(
                    reducer.Reduce(command),
                    reducer.StoreKey.Name,
                    command.Reason,
                    ct);
  
        }

        public ValueTask ExecuteAsync(ICommand command, CancellationToken ct = default)
        {
                if (command is TCommand)
                {
                    return ExecuteAsync((TCommand)command, ct);
                }
            return ValueTask.CompletedTask;
        }
    }
}
