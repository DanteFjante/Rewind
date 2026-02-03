namespace Rewind.Redux.Store;

public interface IStoreInitializer
{
    public Task InitializeStores(CancellationToken ct = default);
}
