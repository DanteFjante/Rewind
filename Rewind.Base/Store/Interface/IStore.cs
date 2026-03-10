namespace Rewind.Store;

public interface IStore
{
    public object? this[string key] { get; }

    public string Type { get; }
    public IEnumerable<string> GetKeys();
    public ValueTask<bool> CreateStateAsync(string key, CancellationToken ct = default);
    public string? GetState(string name = "");
    public SerializableSnapshot? GetSnapshot(string name = "");
    public ValueTask SetState(string serializedState, string name = "", string? reason = null, CancellationToken ct = default);
    public ValueTask SetSnapshot(SerializableSnapshot snapshot, bool silent = false, CancellationToken ct = default);

    public IDisposable Subscribe(Action<StoreKey> listener);
}

public interface IStore<TState> : IStore
{
    public new TState? this[string name] { get; }

    public Snapshot<TState> Snapshot { get; }
    public TState State => Snapshot.State;
    public new Snapshot<TState>? GetSnapshot(string name = "");
    public ValueTask SetSnapshot(Snapshot<TState> snapshot, bool silent = false, CancellationToken ct = default);

    public ValueTask UpdateAsync(Func<TState, TState> reducer, string key = "", string? reason = null, CancellationToken ct = default);

    public IDisposable Subscribe(Action<StoreKey, TState> listener);

}
