using Rewind.Middleware;

namespace Rewind.Store
{
    public static class StoreFactory
    {
        public static IInitializableStore<TState> Create<TState>(TState initialState, StoreKey? key = null, params List<Func<BaseMiddleware<TState>>> middlewareFactories)
        => new Store<TState>(initialState, key, middlewareFactories);

        public static IInitializableStore<TState> Create<TState>(Snapshot<TState> initialState, params List<Func<BaseMiddleware<TState>>> middlewareFactories)
        => new Store<TState>(initialState, middlewareFactories);
    }
}
