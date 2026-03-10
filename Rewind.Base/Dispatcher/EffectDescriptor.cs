namespace Rewind.Effects
{
    public class EffectDescriptor
    {
        public Type CommandType { get; init; }
        public Type EffectType { get; init; }
        public Func<IServiceProvider, IEffect> Factory { get; init; }
    }
}
