using Rewind.Base.Dispatcher.Interface;
using Rewind.Commands;
using Rewind.Effects;
using System.Data;

namespace Rewind.Base.Dispatcher.Internal
{
    public class EffectRepository : IEffectRepository
    {
        Dictionary<Type, List<IEffect>> effects { get; }
        List<EffectDescriptor> descriptors { get; }
        IServiceProvider sp;

        public EffectRepository(IServiceProvider sp, IEnumerable<EffectDescriptor> effectDesciptors)
        {
            descriptors = effectDesciptors?.ToList() ?? new();
            effects = new();
            this.sp = sp;
        }

        public IEnumerable<IEffect<TCommand>> GetEffects<TCommand>() where TCommand : ICommand
        {
            if (!effects.ContainsKey(typeof(TCommand)))
            {
                var list = new List<IEffect>();
                foreach (var effect in descriptors.Where(x => x.CommandType == typeof(TCommand)))
                {
                    list.Add(effect.Factory(sp));
                }
                effects.Add(typeof(TCommand), list);
            }
            return effects[typeof(TCommand)].Cast<IEffect<TCommand>>();
        }

        public IEnumerable<IEffect> GetEffects(Type commandType) 
        {
            if (!effects.ContainsKey(commandType))
            {
                var list = new List<IEffect>();
                foreach (var effect in descriptors.Where(x => x.CommandType == commandType))
                {
                    list.Add(effect.Factory(sp));
                }
                effects.Add(commandType, list);
            }
            return effects[commandType];
        }

        public IEnumerable<IEffect> GetEffects()
        {
            if (!effects.Any() && descriptors.Any())
            {
                foreach (var desc in descriptors)
                {
                    if (effects.ContainsKey(desc.CommandType))
                    {
                        effects[desc.CommandType].Add(desc.Factory(sp));
                    }
                    else
                    {
                        effects[desc.CommandType] = [desc.Factory(sp)];
                    }
                }
            }
            return new List<IEffect>();
        }
    }
}
