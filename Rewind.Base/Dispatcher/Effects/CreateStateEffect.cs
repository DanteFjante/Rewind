using Rewind.Base.Dispatcher.Interface;
using Rewind.Commands;
using Rewind.Store;
using Rewind.Store.Internal.Builders;

namespace Rewind.Effects
{
    public class CreateStateEffect<TState> : IEffect<CreateState<TState>>
    {
        public Type CommandType { get; } = typeof(CreateState<TState>);
        public IStore<TState> Store { get; }
        public IDispatcher Dispatcher { get; }

        public CreateStateEffect(
            IStore<TState> store,
            IDispatcher dispatcher
            )
        {
            //Store = store;
            Dispatcher = dispatcher;
        }


        public async ValueTask HandleAsync(CreateState<TState> command, CancellationToken ct = default)
        {
            await Dispatcher.AddReducer(new Reducer<TState, UpdateState<TState>>(command => command.Reducer, command.StateName), ct);
        }
    }
}
