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
            Store = store;
            Dispatcher = dispatcher;
        }


        public async ValueTask HandleAsync(CreateState<TState> command, CancellationToken ct = default)
        {


            if (Store == null)
                return;

            if (Store[command.StateName] != null)
            {
                return;
            }

            await Store.CreateStateAsync(command.StateName, ct);
            var updateReducer = new Reducer<TState, UpdateState<TState>>(
                command => command.Reducer,
                command.StateName,
                n => n.Equals(command.CommandName));

            await Dispatcher.AddReducer(updateReducer);
        }
    }
}
