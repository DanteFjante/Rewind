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

        public static IStoreBuilder<TState> AddSync<TState>(this IStoreBuilder<TState> store, Action<SyncSettings>? settings = null, bool useNavigationmanager = false)
        {

            return store
                .SetStateManager<ExtendedStateManager>()
                .AddMiddleware<SyncMiddleware<TState>>()
                .AddOptions<SyncSettings>(b => b.Configure(settings))
                .AddService<ISyncService>(b => b.SetImplementationType<ClientSyncService>())
                .AddService<IClientSyncConnection>(b => b.SetImplementationType<ClientSyncConnection>())
                .AddService<UserService>()
                .AddService<HubConnection>(b => b.SetFactory(
                    sc =>
                    {
                        sc.TryAddScoped<HubConnection>(sp =>
                        {
                            Uri uri;
                            if (useNavigationmanager)
                            {
                                var nav = sp.GetRequiredService<NavigationManager>();
                                uri = nav.ToAbsoluteUri(SyncUriRelative);
                            }
                            else
                            {
                                SyncSettings settings = sp.GetRequiredService<SyncSettings>();
                                uri = new UriBuilder(settings.ServerProtocol, settings.ServerAdress, settings.ServerPort, SyncUriRelative).Uri;
                            }
                            return new HubConnectionBuilder()
                            .WithUrl(uri)
                            .WithAutomaticReconnect()
                            .Build();
                        });
                    },
                    sp => sp.GetRequiredService<HubConnection>()));
        }
    }
}
