namespace Rewind.Store
{
    public record StoreState<TState>(
        TState State,
        long Version,
        DateTime UpdatedAt,
        string? Reason);
}
