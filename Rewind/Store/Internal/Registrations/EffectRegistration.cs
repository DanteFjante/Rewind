using Microsoft.Extensions.DependencyInjection;
using Rewind.Effects;

namespace Rewind.Store.Internal.Registrations
{
    public class EffectRegistration
    {
        public required Action<IServiceCollection> EffectSetup { get; set; }
        public required Func<IServiceProvider, IEffect> Factory { get; set; }
        public required Type EffectType { get; set; }
        public required Type CommandType { get; set; }

    }
}
