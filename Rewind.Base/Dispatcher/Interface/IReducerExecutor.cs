using Rewind.Commands;

namespace Rewind.Base.Dispatcher.Interface
{
    public interface IReducerExecutor
    {
        Type CommandType { get; }
        Type StateType { get; }

        ValueTask ExecuteAsync(ICommand command, CancellationToken ct = default);
    }
}
