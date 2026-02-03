using System.Runtime.CompilerServices;

namespace Rewind.Redux.Store;
public interface IInitializableStore
{
    public ValueTask InitializeAsync(CancellationToken ct = default);
    public bool IsInitialized { get; }
}

public interface IInitializableStore<TState> : IInitializableStore { }

