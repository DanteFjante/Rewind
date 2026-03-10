using Rewind.Common;

namespace Rewind.Commands
{
    public class UpdateState<TState> : ICommand
    {
        public Guid CommandId { get; } = Guid.NewGuid();

        public required Func<TState, TState> Reducer { get; init; }
        public required string StateName { get; init; }

        public string? Reason => $"Store of {HelperMethods.StoreType<TState>()} with name: {StateName} has been updated";

        public string CommandName { get; set; } = "";
    }
}
