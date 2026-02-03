namespace Rewind.Store;
public interface IInitializableStore
{
    public ValueTask InitializeAsync(CancellationToken ct = default);
    public bool IsInitialized { get; }
}

public interface IInitializableStore<TState> : IInitializableStore { }

