using Rewind.Commands;

namespace Rewind.Base.Dispatcher.Interface
{
    public interface IReducerExecutor
    {
        Type CommandType { get; }
        Type StateType { get; }
        Predicate<string>? CommandFilter { get; }

        ValueTask ExecuteAsync(ICommand command, CancellationToken ct = default);
    }
}
