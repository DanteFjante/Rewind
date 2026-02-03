namespace Rewind.Store;

public interface IStore
{
    public IDisposable Subscribe(Action listener, bool fireImmediately = true);
}
public interface IStore<TState> : IStore
{
    public StoreState<TState> GetSnapshot();
    public TState State => GetSnapshot().State;
    public ValueTask UpdateAsync(Func<TState, TState> reducer, string? reason = null, CancellationToken ct = default);
    public IDisposable Subscribe(Action<TState> listener, bool fireImmediately = true);

}
