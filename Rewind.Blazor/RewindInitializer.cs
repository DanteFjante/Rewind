using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Rewind.Store;

namespace Rewind.Blazor;

public class RewindInitializer : ComponentBase
{

    [Inject]
    public IEnumerable<IInitializableStore> InitializableStores { get; set; } = new List<IInitializableStore>();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
    }

    protected override Task OnInitializedAsync()
    {
        
        //@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))
        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            List<Task> Tasks = new();
            foreach (var store in InitializableStores)
            {
                Tasks.Add(store.InitializeAsync().AsTask());
            }
            await Task.WhenAll(Tasks);

            StateHasChanged();
        }
    }

}
