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

    }
}
