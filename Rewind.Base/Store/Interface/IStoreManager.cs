using Rewind.Common;

namespace Rewind.Store;

public interface IStoreManager : IStoreProvider
{
    //Returns true if store is added
    public ValueTask<bool> AddStore(IStore store);

    //Returns true if store is removed
    public ValueTask<bool> RemoveStore<TState>() => RemoveStore(HelperMethods.StoreType<TState>());

    //Returns true if store is removed
    public ValueTask<bool> RemoveStore(string storeType);

    //Returns true if provider has store
    public ValueTask<bool> HasStore<TState>();

    //Returns store state version, -1 if no such store or state name exists
    public ValueTask<long?> Version<TState>(string name = ""); 
}
