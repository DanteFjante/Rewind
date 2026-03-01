using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Blazor.Sync;
using Rewind.Extensions.Sync;
using Rewind.Extensions.Sync.Server;
using Rewind.Server.Builders;
using Rewind.Settings;
using Rewind.Sync.Server;

namespace Rewind.Server.Sync
{
    public static class SyncExtensionMethods
    {
        public const string SyncUriRelative = "/rewind-sync";
        public static void MapRewind(this IEndpointRouteBuilder app)
        {
            app.MapHub<SyncHub>(SyncUriRelative);
        }

        public static ServerBuilder AddServerSync(this ServerBuilder storeBuilder) => storeBuilder
            .AddOptions<SyncSettings>(b => b.ReadFromSettings("Sync"))
            .AddService<ISyncService>(b => b.SetImplementationType<ServerSyncService>())
            .AddService<IServerSyncConnection>(b => b.SetFactory(
                sc =>
                {
                    sc.AddSignalR();
                    sc.TryAddScoped<IServerSyncConnection, ServerSyncConnection>();
                    
                },
                sp => sp.GetRequiredService<ServerSyncConnection>()
                )
            );
    }
}
