using Rewind.Redux;
using Rewind.Redux.Middleware;
using Microsoft.Extensions.Options;

namespace Rewind.LocalStorage
{
    public class LocalStorageMiddleware<TState> : BaseMiddleware<TState>
    {
        public ILocalStorage LocalStorage { get; set; }
        public LocalStorageSettings Settings { get; set; }

        public LocalStorageMiddleware(
            ILocalStorage localStorage, IOptionsMonitor<LocalStorageSettings> settings)
        {
            LocalStorage = localStorage;
            Settings = settings.Get(HelperMethods.StoreName<TState>());
        }

        protected override async ValueTask BeforeInitializeStore(InitializeMiddlewareContext<TState> context, InitNextAsync next, CancellationToken ct)
        {
            if (await LocalStorage.HasKeyAsync(Settings.StorageKey))
            {
                var storedState = await LocalStorage.GetItemAsync(Settings.StorageKey, context.State);
                context.State = storedState!;
            }

            await next(context, ct);
        }

        protected override async ValueTask AfterUpdate(UpdateMiddlewareContext<TState> context, UpdateNextAsync next, CancellationToken ct)
        {
            await LocalStorage.SetItemAsync(Settings.StorageKey, context.NextState);
        }
    }

}
