namespace Rewind.Store;

public interface IStoreProvider
{
    public ValueTask<IStore<TState>?> GetStore<TState>();
    public ValueTask<IStore?> GetStore(string storeType);
    public void EnableStoreInitialization();

}
