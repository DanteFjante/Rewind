using Rewind.Common;

namespace Rewind.Commands
{
    public class CreateState<TState> : ICommand
    {
        public Guid CommandId { get; } = Guid.CreateVersion7();

        public required string StateName { get; init; }
        public string? Reason => $"Created State for Store of {HelperMethods.StoreType<TState>()} with name: {StateName}";

    }
}
