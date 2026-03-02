using Microsoft.Extensions.DependencyInjection;
using Rewind.Server.Builders;

namespace Rewind.Server
{
    public static class ServerExtensionsMethods
    {
        public static IServiceCollection AddRewindServer(this IServiceCollection sc, Func<ServerBuilder, ServerBuilder>? builder = null)
        {
            ServerBuilder sb = new ServerBuilder();
            
            if(builder != null)
                sb = builder(sb);

            sb.Build(sc);

            return sc;
        }
    }
}
