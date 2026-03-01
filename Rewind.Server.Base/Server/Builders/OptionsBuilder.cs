using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Rewind.Server.Builders
{
    public class OptionsBuilder<TOptions>
        where TOptions : class
    {
        public Dictionary<string, object> Settings { get; }
        public Action<IServiceCollection>? Provider { get; set; }
        public Func<IServiceProvider, TOptions>? Factory { get; set; }
        public Action<TOptions>? Configuration { get; set; }
        public Type ServiceType => typeof(TOptions);

        public string? SectionName;


        public OptionsBuilder()
        {
            Settings = new();
            UseDefaultSetup();
        }

        public OptionsBuilder<TOptions> Configure(Action<TOptions>? configuration)
        {
            Configuration = configuration;
            return this;
        }

        public OptionsBuilder<TOptions> UseDefaultSetup()
        {

            Provider = sc =>
            {

                sc.AddOptions<TOptions>(SectionName)
                    .Configure<IConfiguration>((opt, config) =>
                    {
                        if (!string.IsNullOrWhiteSpace(SectionName))
                        {
                            var section = config.GetSection(SectionName);

                            if (section.Exists())
                                section.Bind(opt);
                        }

                        Configuration?.Invoke(opt);
                    });

                sc.TryAddScoped((sp) => sp.GetRequiredService<IOptionsMonitor<TOptions>>().Get(SectionName));
            };

            Factory = (sp) =>
            {
                return sp.GetRequiredService<IOptionsMonitor<TOptions>>().Get(SectionName);
            };

            return this;
        }

        public OptionsBuilder<TOptions> SetFactory(Func<IServiceProvider, TOptions> factory)
        {
            Provider = null;
            Factory = factory;
            return this;
        }

        public OptionsBuilder<TOptions> SetFactory(Action<IServiceCollection> provider, Func<IServiceProvider, TOptions> factory)
        {
            Provider = provider;
            Factory = factory;
            return this;
        }

        public OptionsBuilder<TOptions> ReadFromSettings(string sectionName)
        {
            SectionName = sectionName;
            return this;
        }

        internal OptionsRegistration Build()
        {
            OptionsRegistration optionsRegistration = new OptionsRegistration(Provider, Factory!, ServiceType);
            return optionsRegistration;
        }
    }
}
