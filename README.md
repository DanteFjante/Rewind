# Rewind

## What Is Rewind
Rewind is a Redux-inspired state/store toolkit for .NET. It is built around:

- immutable state records
- dispatching commands through reducers and effects
- middleware hooks around initialization and updates
- DI-first composition with fluent builders

This repo currently targets .NET 10 and is split into focused projects.

## Packages and Responsibilities
| Package | Responsibility | Main APIs |
| --- | --- | --- |
| `Rewind.Base` | Core primitives and runtime engine (stores, snapshots, middleware, dispatcher contracts) | `IStore<TState>`, `IInitializableStore<TState>`, `StoreFactory`, `BaseMiddleware<TState>`, `IDispatcher`, `ICommand` |
| `Rewind` | DI registration and fluent builders | `AddDispatcher`, `IDispatcherBuilder`, `IStoreBuilder<TState>`, nested builders |
| `Rewind.Blazor` | Blazor integration and browser-focused extensions | `RewindInitializer`, `StoreComponent`, `AddLocalPersistence`, `AddSync` |
| `Rewind.Extensions` (optional) | Optional middleware and services (logging, persistence, sync plumbing) | `AddLogging`, persistence/sync middleware types |

## Requirements and Installation
- Target framework: `net10.0`
- No NuGet packages are published from this repo yet.
- Use project references.

Example:

```powershell
dotnet add <your-app>.csproj reference Rewind.Base/Rewind.Base.csproj
dotnet add <your-app>.csproj reference Rewind/Rewind.csproj
# Optional:
dotnet add <your-app>.csproj reference Rewind.Extensions/Rewind.Extensions.csproj
dotnet add <your-app>.csproj reference Rewind.Blazor/Rewind.Blazor.csproj
```

## Quickstart (End-to-End)
```csharp
using Microsoft.Extensions.DependencyInjection;
using Rewind.Base.Dispatcher.Interface;
using Rewind.Commands;
using Rewind.Store;

public sealed record CounterState(int Count);

public sealed record IncrementCounter(int Amount, string CommandName = "") : ICommand
{
    public Guid CommandId { get; } = Guid.CreateVersion7();
    public string? Reason => $"Increment by {Amount}";
}

var services = new ServiceCollection();

services.AddDispatcher(d => d
    .RegisterStore(new CounterState(0))
    .RegisterReducer<CounterState, IncrementCounter>(
        cmd => state => state with { Count = state.Count + cmd.Amount }));

await using var provider = services.BuildServiceProvider();

var store = provider.GetRequiredService<IInitializableStore<CounterState>>();
await store.InitializeAsync(); // Required before update/dispatch.

var dispatcher = provider.GetRequiredService<IDispatcher>();
await dispatcher.DispatchAsync(new IncrementCounter(1));

Console.WriteLine(store.State.Count); // 1
```

Alternative initialization flow (manager-based):

```csharp
var storeManager = provider.GetRequiredService<IStoreManager>();
storeManager.EnableStoreInitialization();
var storeFromManager = await storeManager.GetStore<CounterState>();
```

## DispatcherBuilder Guide
`AddDispatcher` is the main entrypoint in `Rewind`:

```csharp
services.AddDispatcher(d => d
    .RegisterStore(() => new CounterState(0))
    .RegisterReducer<CounterState, IncrementCounter>(
        cmd => state => state with { Count = state.Count + cmd.Amount }));
```

### Configure managers
```csharp
using Rewind.Base.Store.Implementation;

services.AddDispatcher(d => d
    .SetStoreManager(_ => new BaseStoreManager())
    .SetStateManager(_ => new BaseStateManager())
    .RegisterStore(new CounterState(0)));
```

### Register effects
```csharp
using Rewind.Effects;

public sealed class CounterAuditEffect : IEffect<IncrementCounter>
{
    public Type CommandType => typeof(IncrementCounter);

    public ValueTask HandleAsync(IncrementCounter command, CancellationToken ct = default)
    {
        Console.WriteLine($"Handled: {command.Reason}");
        return ValueTask.CompletedTask;
    }
}

services.AddDispatcher(d => d
    .RegisterStore(new CounterState(0))
    .RegisterEffect<CounterAuditEffect, IncrementCounter>());
```

Factory overload:

```csharp
services.AddDispatcher(d => d
    .RegisterStore(new CounterState(0))
    .RegisterEffect<CounterAuditEffect, IncrementCounter>(sp => new CounterAuditEffect()));
```

### Named state creation with built-in commands
`RegisterStore<TState>(...)` also wires built-in `CreateState<TState>` and `UpdateState<TState>` handling.

```csharp
await dispatcher.DispatchAsync(new CreateState<CounterState>("session-1", "session-1"));

await dispatcher.DispatchAsync(new UpdateState<CounterState>
{
    StateName = "session-1",
    CommandName = "session-1",
    Reducer = s => s with { Count = s.Count + 10 }
});
```

## StoreBuilder and Nested Builders Guide
`RegisterStore` accepts a store builder callback:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Rewind.Middleware;

public sealed class CounterOptions
{
    public int Step { get; set; } = 1;
}

public interface ICounterClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemCounterClock : ICounterClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class GuardMiddleware : BaseMiddleware<CounterState>
{
    protected override ValueTask BeforeUpdate(
        UpdateMiddlewareContext<CounterState> context,
        UpdateNextAsync next,
        CancellationToken ct)
    {
        var projected = context.Reducer(context.CurrentState);
        if (projected.Count < 0)
            context.Block("Counter cannot go below zero.");

        return ValueTask.CompletedTask;
    }
}

services.AddDispatcher(d => d.RegisterStore(
    () => new CounterState(0),
    store => store
        .AddMiddleware<GuardMiddleware>(mw => mw
            .UseDefaultSetup()
            .SetLifeTime(ServiceLifetime.Scoped))
        .AddService<ICounterClock>(svc => svc
            .SetImplementationType<SystemCounterClock>()
            .SetLifetime(ServiceLifetime.Singleton))
        .AddService<ICounterClock>(svc => svc
            .SetServiceKey("utc")
            .SetFactory(sp => new SystemCounterClock()))
        .AddOptions<CounterOptions>(opt => opt
            .SetStoreName("CounterStore")
            .ReadFromSettings("Rewind:Counter")
            .Configure(o => o.Step = 2))
        .AddStoreDecorator((sp, createdStore) =>
        {
            // See caveats section for current behavior.
        })));
```

### `IMiddlewareBuilder<TState, TMiddleware>`
```csharp
store.AddMiddleware<GuardMiddleware>(mw => mw
    .UseDefaultSetup()
    .SetLifeTime(ServiceLifetime.Singleton));

store.AddMiddleware<GuardMiddleware>(mw => mw
    .SetFactory(sp => new GuardMiddleware()));
```

### `IServiceBuilder<TState, TService>`
```csharp
store.AddService<ICounterClock>(svc => svc
    .SetImplementationType<SystemCounterClock>()
    .SetLifetime(ServiceLifetime.Singleton));

store.AddService<ICounterClock>(svc => svc
    .SetFactory(
        sc => { /* optional custom registrations */ },
        sp => new SystemCounterClock()));
```

### `IOptionsBuilder<TState, TOptions>`
```csharp
store.AddOptions<CounterOptions>(opt => opt
    .SetStoreName("CounterStore")
    .ReadFromSettings("Rewind:Counter")
    .Configure(o => o.Step = 5));

store.AddOptions<CounterOptions>(opt => opt
    .Setup(sc =>
    {
        sc.AddOptions<CounterOptions>("CounterStore")
          .Configure(o => o.Step = 10);
    }));
```

Example `appsettings.json` for `ReadFromSettings("Rewind:Counter")`:

```json
{
  "Rewind": {
    "Counter": {
      "Step": 1,
      "Your.Namespace.CounterState": {
        "Step": 5
      }
    }
  }
}
```

## Using Rewind.Base Directly
You can use `Rewind.Base` without DI/builders.

```csharp
using Rewind.Middleware;
using Rewind.Store;

var store = StoreFactory.Create(
    () => new CounterState(0),
    new List<Func<BaseMiddleware<CounterState>>>
    {
        () => new GuardMiddleware()
    });

await store.InitializeAsync();
await store.UpdateAsync(s => s with { Count = s.Count + 1 }, reason: "Manual increment");

Snapshot<CounterState>? snapshot = store.GetSnapshot();
SerializableSnapshot serializable = snapshot!.ToSerializableSnapshot();
StoreKey key = serializable.Key;

Console.WriteLine($"{key.Type}:{key.Name} v{serializable.Version}");
```

Middleware context example:

```csharp
public sealed class GuardMiddleware : BaseMiddleware<CounterState>
{
    protected override ValueTask BeforeInitializeStore(
        InitializeMiddlewareContext<CounterState> context,
        InitNextAsync next,
        CancellationToken ct)
    {
        if (context.State.Count < 0)
            context.State = context.State with { Count = 0 };

        return ValueTask.CompletedTask;
    }

    protected override ValueTask BeforeUpdate(
        UpdateMiddlewareContext<CounterState> context,
        UpdateNextAsync next,
        CancellationToken ct)
    {
        var projected = context.Reducer(context.CurrentState);
        if (projected.Count < 0)
            context.Block("Counter cannot go below zero.");

        return ValueTask.CompletedTask;
    }
}
```

## Blazor Components
### Root-level initialization with `RewindInitializer`
`RewindInitializer` initializes all injected `IInitializableStore` instances after first render.

```razor
@using Rewind.Blazor

<RewindInitializer />
<Routes />
```

### Component-level initialization with `StoreComponent`
`StoreComponent` discovers `[Inject]` properties that implement `IInitializableStore` and initializes them.

```razor
@page "/counter"
@using Rewind.Store
@inherits Rewind.Blazor.StoreComponent
@inject IInitializableStore<CounterState> CounterStore

@if (!StoresReady)
{
    <p>Initializing stores...</p>
}
else
{
    <p>Count: @CounterStore.State.Count</p>
}

@code {
    protected override async Task OnInitializedAsync(bool storesInitialized)
    {
        if (storesInitialized)
        {
            await CounterStore.UpdateAsync(
                s => s with { Count = s.Count + 1 },
                reason: "Counter page opened");
        }
    }
}
```

## Optional Extensions
Optional extension methods live in `Rewind.Extensions`/`Rewind.Blazor`.

```csharp
using Rewind.Blazor.Persistence;
using Rewind.Blazor.Sync;
using Rewind.Logging;

services.AddDispatcher(d => d.RegisterStore(
    () => new CounterState(0),
    store => store
        .AddLogging()
        .AddLocalPersistence()
        .AddSync(sync =>
        {
            sync.ServerProtocol = "https";
            sync.ServerAdress = "api.example.com";
            sync.ServerPort = 443;
        }, useAuth: true)));
```

- `AddLogging()` adds `LoggingMiddleware<TState>`.
- `AddLocalPersistence()` wires persistence middleware for browser local storage.
- `AddSync(...)` wires client sync middleware + SignalR connection services.

## Current Caveats / Known Gaps
- Stores must be initialized before `UpdateAsync`, `SetState`, `SetSnapshot`, and dispatch paths that update state.
- `AddStoreDecorator(...)` is currently not exercised by the `AddDispatcher` registration pipeline.
- `IMiddlewareBuilder.SetFactory(Action<IServiceCollection>, ...)` currently does not apply the provided setup action as expected.
- `AddSync(..., useAuth)` currently does not branch behavior based on `useAuth`.
- `PersistenceSettings` is currently a placeholder type with no defined properties.
