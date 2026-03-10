using Rewind.Store;

namespace Rewind.Base.Dispatcher.Interface
{
    public interface IReducerManager
    {
        public IEnumerable<IReducer> GetReducers<TCommand>(string? commandName = null);
        public IEnumerable<IReducer> GetReducers(Type commandType, string? commandName = null);

        public void AddReducer(IReducer reducer);

        public void RemoveReducer(Type commandType, StoreKey storeKey);
    }
}
