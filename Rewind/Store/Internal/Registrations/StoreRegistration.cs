using Microsoft.Extensions.DependencyInjection;

namespace Rewind.Store.Internal.Registrations
{
    public class StoreRegistration
    {
        public Func<IServiceProvider, IStore> StoreFactory { get; set; }
        public Action<IServiceCollection> StoreSetup { get; set; }

        public Type TState { get; set; }
    }
}
