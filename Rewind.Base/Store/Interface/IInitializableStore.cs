namespace Rewind.Store;
public interface IInitializableStore : IStore, IDisposable
{
    public ValueTask InitializeAsync(CancellationToken ct = default);
    public bool IsInitialized { get; }
    public bool IsDisposed { get; }

    public IDisposable SubscribeOnInitialized(Action<IInitializableStore> onInitialized);
    public IDisposable SubscribeOnDisposed(Action<StoreKey> onDisposed);
}

public interface IInitializableStore<TState> : IInitializableStore, IStore<TState> { }

