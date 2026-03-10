using Rewind.Base.Dispatcher.Interface;
using Rewind.Store;

namespace Rewind.Base.Dispatcher.Internal
{
    public class ReducerManager : IReducerManager
    {
        private List<IReducer> reducers { get; set; }

        public ReducerManager()
        {
            reducers = new List<IReducer>();
        }

        public ReducerManager(List<IReducer> reducers)
        {
            this.reducers = reducers;
        }

        public IEnumerable<IReducer> GetReducers<TCommand>(string? commandName = null)
        {
            var rs = reducers.Where(x => typeof(TCommand) == x.CommandType);

            return rs;

        }

        public IEnumerable<IReducer> GetReducers(Type commandType, string? commandName = null)
        {
            var rs = reducers.Where(x => commandType == x.CommandType);

            return rs;
        }

        public void AddReducer(IReducer reducer)
        {
            reducers.Add(reducer);
        }

        public void RemoveReducer(Type commandType, StoreKey storeKey)
        {
            reducers.RemoveAll(x => x.CommandType == commandType && x.StoreKey == storeKey);
        }
    }
}
