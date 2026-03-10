using Rewind.Commands;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Common;

namespace Rewind.Store.Internal.Builders
{
    public delegate Func<TState, TState> Reduction<TState, TCommand>(TCommand command)
        where TCommand : ICommand;

    public class Reducer<TState, TCommand> : IReducer<TState, TCommand>
        where TCommand : ICommand
    {

        public StoreKey StoreKey { get; }

        public Type CommandType { get; }
        public Predicate<string>? CommandFilter { get; }

        private Reduction<TState, TCommand> reducer;
        public Reducer(Reduction<TState, TCommand> reducer, string stateName = "", Predicate<string>? commandFilter = null)
        {
            CommandType = typeof(TCommand);

            StoreKey = new StoreKey(HelperMethods.StoreType<TState>(), stateName);
            this.reducer = reducer;
            CommandFilter = commandFilter;
        }
        public Reducer(Reduction<TState, TCommand> reducer, Predicate<string>? commandFilter, string stateName = "")
        {
            CommandType = typeof(TCommand);
            StoreKey = new StoreKey(HelperMethods.StoreType<TState>(), stateName);
            this.reducer = reducer;
        }

        public Func<TState, TState> Reduce(TCommand command)
        {
            return reducer(command);
        }
    }
}
