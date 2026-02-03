namespace Rewind.Store;

public interface IStoreInitializer
{
    public Task InitializeStoresAsync(CancellationToken ct = default);
}
