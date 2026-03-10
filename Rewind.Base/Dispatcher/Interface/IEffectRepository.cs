using Rewind.Commands;
using Rewind.Effects;

namespace Rewind.Base.Dispatcher.Interface
{
    public interface IEffectRepository
    {
        public IEnumerable<IEffect<TCommand>> GetEffects<TCommand>()
            where TCommand : ICommand;

        public IEnumerable<IEffect> GetEffects();
    }
}
