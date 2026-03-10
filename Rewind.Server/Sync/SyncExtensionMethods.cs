using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rewind.Extensions.Sync;
using Rewind.Server.Builders;
using Rewind.Server.Sync.Builder;
using Rewind.Settings;

namespace Rewind.Server.Sync
{
    public static class SyncExtensionMethods
    {
        public const string SyncUriRelative = "/rewind-sync";
        public static MapBuilder UseSync(this MapBuilder mapBuider)
        {
            mapBuider.AddApplicationAction(rb => rb.MapHub<SyncHub>(SyncUriRelative).RequireAuthorization().AllowAnonymous());
            return mapBuider;
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
