using Rewind.Common;

namespace Rewind.Commands
{
    public class CreateState<TState> : ICommand
    {
        public Guid CommandId { get; } = Guid.CreateVersion7();

        public string StateName { get; init; }
        public string? Reason => $"Created State for Store of {HelperMethods.StoreType<TState>()} with name: {StateName}";

        public string CommandChannel { get; set; } = "";

        public CreateState(string stateName, string? commandName = null)
        {
            StateName = stateName;
            CommandChannel = commandName ?? stateName;
        }
    }
}
