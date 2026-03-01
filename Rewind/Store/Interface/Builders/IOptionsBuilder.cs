using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Store.Builders
{

    public interface IOptionsBuilder<TState, TOptions>
        where TOptions : class
    {
        public IOptionsBuilder<TState, TOptions> Configure(Action<TOptions>? dynamicSettings = null);

        public IOptionsBuilder<TState, TOptions> UseDefaultSetup();
        public IOptionsBuilder<TState, TOptions> Setup(Action<IServiceCollection> provider);
        public IOptionsBuilder<TState, TOptions> SetStoreName(string settingsName);
        public IOptionsBuilder<TState, TOptions> ReadFromSettings(string sectionName);

        internal OptionsRegistration Build();

    }
}
