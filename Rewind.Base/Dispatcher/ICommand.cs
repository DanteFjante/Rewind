namespace Rewind.Commands
{
    public interface ICommand
    {
        Guid CommandId { get; }
        string? Reason { get; }

        string CommandName { get; }
    }


}
