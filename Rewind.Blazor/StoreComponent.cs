using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Rewind
{
    public class StoreComponent : ComponentBase
    {

        private List<Type> _injectedStores = new();

        protected override async Task OnInitializedAsync()
        {
            
        }
        private static IEnumerable<Type> FindInjectedStoreTypes(object component)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Scan properties on the *runtime type* (derived component), not just StoreComponent.
            foreach (var prop in component.GetType().GetProperties(flags))
            {
                // Only consider DI-injected properties.
                if (!prop.IsDefined(typeof(InjectAttribute), inherit: true))
                    continue;

                var t = prop.PropertyType;

                // Match IStore<T>
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IStore<>))
                {
                    yield return t; // e.g., IStore<MyState>
                }
            }
        }
    }
}
