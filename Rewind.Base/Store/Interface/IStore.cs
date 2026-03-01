namespace Rewind.Store;

public interface IStore
{
    public StoreKey Key { get; }
    public long Version { get; }
    public DateTime UpdatedAt { get; }
    public string? Reason { get; }
    public string GetState();
    public SerializableSnapshot GetSnapshot();
    public ValueTask SetState(string serializedState, string reason, CancellationToken ct = default);
    public ValueTask SetSnapshot(SerializableSnapshot snapshot, bool silent = false, CancellationToken ct = default);

    public IDisposable Subscribe(Action listener);



}
public interface IStore<TState> : IStore
{
    public Snapshot<TState> Snapshot { get; }
    public TState State => Snapshot.State;
    public ValueTask SetSnapshot(Snapshot<TState> snapshot, bool silent = false, CancellationToken ct = default);

    public ValueTask UpdateAsync(Func<TState, TState> reducer, string? reason = null, CancellationToken ct = default);

    public IDisposable Subscribe(Action<TState> listener, bool fireImmediately = true);

}
