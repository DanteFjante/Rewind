using Rewind.Commands;

namespace Rewind.Base.Dispatcher.Interface
{
    public interface IDispatcher
    {

        public ValueTask DispatchAsync<TCommand>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand;

        public ValueTask<bool> AddReducer<TState, TCommand>(IReducer<TState, TCommand> reducer, CancellationToken ct = default)
            where TCommand : ICommand;

        public IDisposable SubscribeCommand<TCommand>(Action<TCommand> action)
            where TCommand : ICommand;
    }
}
