using Rewind.Commands;

namespace Rewind.Effects
{
    public interface IEffect
    {
        public Type CommandType { get; }
    }

    public interface IEffect<TCommand> : IEffect where TCommand : ICommand
    {
        public ValueTask HandleAsync(TCommand command, CancellationToken ct = default);
    }
}
