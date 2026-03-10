using Rewind.Commands;
using Rewind.Store;

namespace Rewind.Base.Dispatcher.Interface
{

    public interface IReducer
    {
        public StoreKey StoreKey { get; }
        public Type CommandType { get; }
    }

    public interface IReducer<TState, TCommand> : IReducer
        where TCommand : ICommand
    {
        public Func<TState, TState> Reduce(TCommand command);
    }

}
