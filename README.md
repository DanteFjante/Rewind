# Rewind

A Redux-like state/store library for .NET with a bias toward **low boilerplate** and **fast setup**. Rewind is built around:

- immutable state objects
- DI-first setup
- optional middleware (logging, persistence, custom)
- optional store initialization strategies (eager or per-store)

> Status: early-stage. No releases/packages are published from this repo yet.

---

## Why Rewind

Rewind exists for the “I want predictable state + middleware + DI, but I don’t want to wire 40 abstractions” use case.

Design goals:

- Minimal ceremony to register a store
- Middleware pipeline as the extension point (logging/persistence/custom)
- Store-specific configuration via `IOptions*` patterns

Non-goals (at least currently):

- Being a full Redux clone with reducers/actions semantics identical to JavaScript Redux
- Being opinionated about UI binding (Blazor components support is planned, not shipped)

---

## Installation

### Reference the project

Since there are no NuGet packages published, add a project reference to `Rewind.csproj` in your solution.



---

## Quickstart

### 1) Create an immutable state

Example `CounterState`:

```csharp
public record class CounterState(int Count)
{
    public CounterState Increment() => this with { Count = Count + 1 };
}
```

### 2) Register the store in DI

```csharp
builder.Services.AddStore(new CounterState(0));
```

### 3) Initialize stores

You have two supported initialization styles:

**Initialize all stores at once**

```csharp
// Recommended in Blazor in App.razor so LocalStorage has time to load.
storeInitializer.InitializeStores();
```

**Initialize a store on demand**

```csharp
await initializableStore.InitializeAsync();
```

---

## Middleware

You can attach middleware during `AddStore` registration.

### Built-in extensions

```csharp
builder.Services.AddStore(new CounterState(0), store =>
    store.AddLogging()
         .AddPersistence());
```

### Add your own middleware

```csharp
builder.Services.AddStore(new CounterState(0), store =>
    store.AddMiddleware<LoggingMiddleware<CounterState>>());
```

Rewind treats logging and persistence as middleware, and the same mechanism is intended for user-defined middleware too.

---

## Store-scoped services and options

You can register services and options “inside” the store builder. If you don’t use factory methods, registrations flow through normal DI (typically scoped).

### Services

```csharp
builder.Services.AddStore(new CounterState(0), store =>
    store.AddMiddleware<LocalStorageMiddleware<CounterState>>()
         .AddService<ILocalStorage, LocalStorage>());
```

### Options

```csharp
builder.Services.AddStore(new CounterState(0), store =>
    store.AddOptions<LocalStorageSettings>());
```

You can consume these with `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` as usual.

---

## LocalStorage configuration

LocalStorage is configured via an options section:

- The section name is: `Rewind.LocalStorage.LocalStorageSettings`
- The per-store key is the **full name of the state type**
- You can define a “generic” fallback object, but for LocalStorage this may break behavior (per the current README)

Example `appsettings.json`:

```json
{
  "Rewind.LocalStorage.LocalStorageSettings": {
    "Your.NameSpace.Here.CounterState": {
      "StorageKey": "Counter2"
    }
  }
}
```

---

## Factory-based registration (advanced)

If you want middleware/services/options creation without relying entirely on Microsoft DI conventions, the store builder supports factory methods. This also enables retrieving store-scoped registrations through a store factory.

```csharp
builder.Services.AddStore(new CounterState(0), store => store
    .AddService<ILocalStorage, LocalStorage>()
    .AddOptions<LocalStorageSettings>()
    .AddMiddleware<LocalStorageMiddleware<CounterState>>(
        serviceCollection =>
        {
            // Optional setup without needing to depend on Microsoft DI patterns.
        },
        (serviceProvider, storeFactory) =>
        {
            // Construct middleware using store-scoped services/options
            return new LocalStorageMiddleware<CounterState>(
                storeFactory.GetService<ILocalStorage>(),
                storeFactory.GetOptionsMonitor<LocalStorageSettings>());
        }));
```

---

## Roadmap

Planned features mentioned in the repo:

- Blazor component support
- Middleware dependency handling (middleware depending on other middleware)
- A `CollectionStore` to store multiple instances of the same state type
- Support for non-.NET projects (exploratory)
- Move LocalStorage and Logging middleware/services to separate libraries
- “Rewind” capabilities (undo/redo style)

---

## Contributing

Issues and PRs welcome. If you add features, include:

- a minimal repro/sample
- tests (if/when test project is added)
- README updates if you change the public surface

---
