namespace Rewind.Store;

public interface IStoreInitializer
{
    public Task InitializeStores(CancellationToken ct = default);
}
