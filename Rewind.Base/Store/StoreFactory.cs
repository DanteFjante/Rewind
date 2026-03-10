using Rewind.Middleware;

namespace Rewind.Store
{
    public static class StoreFactory
    {
        public static IInitializableStore<TState> Create<TState>(Func<TState> initialState, params List<Func<BaseMiddleware<TState>>> middlewareFactories)
            where TState : class
            => new Store<TState>(initialState, middlewareFactories);
    }
}
