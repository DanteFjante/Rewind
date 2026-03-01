using Rewind.Middleware;

namespace Rewind.Extensions.Persistence;


public class PersistenceMiddleware<TState> : BaseMiddleware<TState> 
{
    public IPersistanceService Storage { get; set; }
    public PersistenceSettings Settings { get; set; }

    public PersistenceMiddleware(IPersistanceService storage, PersistenceSettings settings)
    {
        Storage = storage;
        Settings = settings;
    }

    protected override async ValueTask BeforeInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
    {
        var key = new PersistenceKey(context.StoreKey);
        if (await Storage.HasStateAsync(key))
        {
            var persistence = await Storage.GetStateAsync(key);
            if (persistence != null)
            {
                context.ApplySnapshot(persistence.ToSnapshot());
            }
        }

        await next(context, ct);
    }

    protected override async ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
    {
        var key = new PersistenceKey(context.StoreKey);
        var persistenceModel = new PersistenceData(context.ToSerializableSnapshot());

        await Storage.SetStateAsync(persistenceModel);

        await next(context, ct);
    }
}
