using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Extensions.Store;
using Rewind.Extensions.Sync;
using Rewind.Extensions.Sync.Client;
using Rewind.Extensions.Users;
using Rewind.Settings;
using Rewind.Store.Builders;
using Rewind.Sync;
using Rewind.Sync.Client;

namespace Rewind.Blazor.Sync
{
    public static class SyncExtensionsMethods
    {
        public const string SyncUriRelative = "/rewind-sync";

        public static IStoreBuilder<TState> AddSync<TState>(this IStoreBuilder<TState> store, Action<SyncSettings>? settings = null, bool useAuth = false)
        {

            return store
                .AddMiddleware<SyncMiddleware<TState>>()
                .AddOptions<SyncSettings>(b => b.Configure(settings).ReadFromSettings("Sync"))
                .AddService<HttpClient>()
                .AddService<ISyncService>(b => b.SetImplementationType<ClientSyncService>())
                .AddService<IClientSyncConnection>(b => b.SetImplementationType<ClientSyncConnection>())
                .AddService<UserService>()
                .AddService<ITokenProvider>(b => b.SetImplementationType<UserService>())
                .AddService<HubConnection>(b => b.SetFactory(
                    sc =>
                    {
                        sc.TryAddScoped<HubConnection>(sp =>
                        {
                            SyncSettings settings = sp.GetRequiredService<SyncSettings>();
                            UserService? userService = sp.GetService<UserService>();
                            Uri uri;
                            var nav = sp.GetService<NavigationManager>();

                            if (nav != null)
                            {
                                uri = SelectHubUrl(settings, nav, SyncUriRelative);
                            }
                            else
                            {
                                uri = new UriBuilder(
                                    settings.ServerProtocol, 
                                    settings.ServerAdress, 
                                    settings.ServerPort, 
                                    SyncUriRelative).Uri;
                            }
                            return new HubConnectionBuilder()
                            .WithUrl(uri, o =>
                            {
                                o.AccessTokenProvider = async () =>
                                {
                                    if (userService == null)
                                        return null;
                                    var login = await userService!.LoginUser();
                                    return login?.Token;

                                };
                            })
                            .WithAutomaticReconnect()
                            .Build();
                        });
                    },
                    sp => sp.GetRequiredService<HubConnection>()));
        }
        static Uri SelectHubUrl(SyncSettings o, NavigationManager nav, string syncUriRelative)
        {
            var clientHost = new Uri(nav.BaseUri).Host; // "localhost" or something else

            var clientUri = new UriBuilder(o.ServerProtocol, "localhost", o.ServerPort, syncUriRelative).Uri;
            var serverUri = new UriBuilder(o.ServerProtocol, o.ServerAdress, o.ServerPort, syncUriRelative).Uri;

            if (clientHost.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return clientUri;

            return serverUri;
        }
    }
}
