using Rewind.Base.Dispatcher.Interface;
using Rewind.Commands;
using Rewind.Effects;
using Rewind.Store;
using Rewind.Store.Builders;
using Rewind.Store.Internal.Builders;

namespace Rewind.Base.Store.Interface
{
    public interface IDispatcherBuilder
    {
        public IDispatcherBuilder SetStoreManager(Func<IServiceProvider, IStoreManager> factory);
        public IDispatcherBuilder SetStoreManager<StoreManager>() where StoreManager : class, IStoreManager;
        public IDispatcherBuilder SetStateManager(Func<IServiceProvider, IStateManager> factory);
        public IDispatcherBuilder SetStateManager<StateManager>() where StateManager : class, IStateManager;

        public IDispatcherBuilder RegisterEffect<TEffect, TCommand>()
            where TEffect : class, IEffect
            where TCommand : ICommand;
        public IDispatcherBuilder RegisterEffect<TEffect, TCommand>(Func<IServiceProvider, TEffect> factory)
            where TEffect : class, IEffect
            where TCommand : ICommand;

        public IDispatcherBuilder RegisterReducer<TState, TCommand>(IReducer<TState, TCommand> reducer)
            where TCommand : ICommand;

        public IDispatcherBuilder RegisterReducer<TState, TCommand>(Reduction<TState, TCommand> reducer, string stateName = "")
            where TCommand : ICommand;

        public IDispatcherBuilder RegisterStore<TState>(Func<TState> InitialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? builder = null)
            where TState : class;
        public IDispatcherBuilder RegisterStore<TState>(TState InitialState, Func<IStoreBuilder<TState>, IStoreBuilder<TState>>? builder = null)
            where TState : class;

    }
}
