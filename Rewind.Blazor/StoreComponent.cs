using Microsoft.AspNetCore.Components;
using Rewind.Store;
using System.Reflection;

namespace Rewind.Blazor;

public class StoreComponent : ComponentBase
{
    private readonly List<IInitializableStore> _initializableStores = new();

    public bool StoresReady { get; set; } = false;

    protected override sealed async Task OnInitializedAsync()
    {
        StoresReady = false;

        _initializableStores.Clear();

        // Initialize any injected stores that are initializable.
        DiscoverInjectedStores(this, _initializableStores);

        bool success = true;
        foreach (var store in _initializableStores)
        {
            if (!store.IsInitialized && !store.IsDisposed)
            {
                try
                {
                    await store.InitializeAsync();
                }
                catch (Exception)
                {
                    success = false;
                }
            }
        }

        StoresReady = success;

        await OnInitializedAsync(success);


    }

    protected virtual Task OnInitializedAsync(bool storesInitialized)
        => Task.CompletedTask;


    private static void DiscoverInjectedStores(object component, List<IInitializableStore> target)
    {
        var props = component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var p in props)
        {
            if (p.GetCustomAttribute<InjectAttribute>() is null)
                continue;

            if (p.GetValue(component) is IInitializableStore init)
                target.Add(init);
        }
    }
}
