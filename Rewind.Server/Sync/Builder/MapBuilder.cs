using Microsoft.AspNetCore.Builder;

namespace Rewind.Server.Sync.Builder
{
    public class MapBuilder
    {
        List<Action<WebApplication>> mappings { get; } = new();
        public MapBuilder AddApplicationAction(Action<WebApplication> appAction)
        {
            mappings.Add(appAction);

            return this;
        }

        public void Build(WebApplication routeBuilder)
        {
            foreach (var mapping in mappings)
            {
                mapping(routeBuilder);
            }
        }
    }
}
