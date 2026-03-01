using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rewind.Common;

namespace Rewind.Store.Builders
{
    internal class OptionsBuilder<TState, TOptions> : IOptionsBuilder<TState, TOptions>
        where TOptions : class
    {
        public Dictionary<string, object> Settings { get; }
        public Action<IServiceCollection>? Provider { get; set; }
        public Action<TOptions>? Configuration { get; set; }
        public Type ServiceType => typeof(TOptions);

        public string? StoreName;
        public string? SectionName;


        public OptionsBuilder()
        {
            Settings = new();
            UseDefaultSetup();
        }

        public IOptionsBuilder<TState, TOptions> Configure(Action<TOptions>? configuration)
        {
            Configuration = configuration;
            return this;
        }

        public IOptionsBuilder<TState, TOptions> UseDefaultSetup()
        {
            string storeName = StoreName;
            if (storeName == null)
                storeName = HelperMethods.StoreType<TState>();

            Provider = sc =>
            {
                sc.AddOptions<TOptions>(StoreName)
                    .Configure<IConfiguration>((opt, config) =>
                    {
                        if (!string.IsNullOrWhiteSpace(SectionName))
                        {
                            var section = config.GetSection(SectionName);
                            var sectionPerState = config.GetSection($"{SectionName}:{storeName}");

                            if (section.Exists())
                                section.Bind(opt);

                            if (sectionPerState.Exists())
                                sectionPerState.Bind(opt);
                        }

                        Configuration?.Invoke(opt);
                    });
                if(!string.IsNullOrWhiteSpace(StoreName))
                {
                    sc.TryAddKeyedScoped(StoreName, (sp, key) => sp.GetRequiredService<IOptionsMonitor<TOptions>>().Get(StoreName));
                }
                else
                sc.TryAddScoped(sp => sp.GetRequiredService<IOptions<TOptions>>().Value);
            };

            return this;
        }

        public IOptionsBuilder<TState, TOptions> Setup(Action<IServiceCollection> provider)
        {
            Provider = provider;
            return this;
        }
        public IOptionsBuilder<TState, TOptions> SetStoreName(string storeName)
        {
            StoreName = storeName;
            return this;
        }

        public IOptionsBuilder<TState, TOptions> ReadFromSettings(string sectionName)
        {
            SectionName = sectionName;
            return this;
        }

        public OptionsRegistration Build()
        {
            OptionsRegistration optionsRegistration = new OptionsRegistration(Provider, ServiceType);
            return optionsRegistration;
        }
    }
}
