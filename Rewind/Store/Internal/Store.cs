using Rewind.Middleware;

namespace Rewind.Store.Internal;

internal class Store<TState> : IStore<TState>, IInitializableStore<TState>, IDisposable
{
    private StoreState<TState> _current;
    private readonly Dictionary<Guid, Action<TState>> _subscribers;
    private readonly object _gate = new();
    private bool _disposed;
    private IEnumerable<BaseMiddleware<TState>> _middleware;
    private BaseMiddleware<TState>.UpdateNextAsync _updatePipeline;
    private BaseMiddleware<TState>.InitNextAsync _initPipline;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;
    public StoreState<TState> GetSnapshot()
    {
        lock (_gate) return _current;
    }
    public Store(TState initial, IEnumerable<BaseMiddleware<TState>>? middleware)
    {
        _subscribers = new Dictionary<Guid, Action<TState>>();
        _current = new StoreState<TState>(initial, 0, DateTime.UtcNow, "Initialized State");
        _middleware = middleware ?? new List<BaseMiddleware<TState>>();

        _initPipline = BuildInitPipeline();
        _updatePipeline = BuildUpdatePipeline();
        
    } 

    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (_disposed)
                throw new InvalidOperationException("Can not initialize a disposed store");
        }

        var context = new InitializeMiddlewareContext<TState>(_current.State);
        await _initPipline(context, ct);

        lock (_gate)
        {
            _current = _current with
            {
                Reason = "Initalized",
                UpdatedAt = DateTime.UtcNow,
                State = context.State,
                Version = 0
            };
            _isInitialized = true;
        }
    }
    public async ValueTask UpdateAsync(Func<TState, TState> reducer, string? reason = null, CancellationToken ct = default)
    {
        Action<TState>[] listeners;
        TState oldValue;

        lock (_gate)
        {
            ThrowIfDisposed();
            oldValue = _current.State;
        }
        
        var context = new UpdateMiddlewareContext<TState>(reducer, oldValue, reason ?? "");

        await _updatePipeline(context, ct);

        if (context.Blocked)
            return;

        var newState = context.NextState ?? context.Reducer(oldValue);

        if (ReferenceEquals(oldValue, newState))
            return;

        lock (_gate)
        {
            _current = _current with
            {
                Reason = reason,
                UpdatedAt = DateTime.UtcNow,
                State = newState,
                Version = _current.Version + 1
            };
            listeners = _subscribers.Values.ToArray();
        }

        foreach (var action in listeners)
        {
            try
            {
                action.Invoke(newState);
            }
            catch { }
        }

    }

    public IDisposable Subscribe(Action<TState> listener, bool fireImmediately = true)
    {
        Subscription subscription;
        TState state;
        lock (_gate)
        {
            ThrowIfDisposed();
            subscription = new Subscription(this);
            _subscribers.Add(subscription.Id, listener);
            state = _current.State;
        }

        if (fireImmediately)
        {
            try
            {
                listener.Invoke(state);
            }
            catch { }
        }

        return subscription;
    }

    private void Unsubscribe(Guid id)
    {
        lock (_gate)
        {
            if (_disposed) return;
            _subscribers.Remove(id);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _subscribers.Clear();
            _disposed = true;
        }
    }

    private BaseMiddleware<TState>.InitNextAsync BuildInitPipeline()
    {
        BaseMiddleware<TState>.InitNextAsync next = (context, ct) => ValueTask.CompletedTask;
        
        foreach (var middleware in _middleware.Reverse())
        {
            var temp = next;
            next = (context, ct) => middleware.InitializeAsync(context, temp, ct);
        }

        return next;
    }

    private BaseMiddleware<TState>.UpdateNextAsync BuildUpdatePipeline()
    {
        BaseMiddleware<TState>.UpdateNextAsync next = (context, ct) =>
        {
            if (context.Blocked)
            {
                return ValueTask.CompletedTask;
            }
            context.NextState = context.Reducer(context.CurrentState);
            return ValueTask.CompletedTask;
        };

        foreach (var middleware in _middleware.Reverse())
        {
            var temp = next;
            next = (context, ct) => middleware.UpdateAsync(context, temp, ct);
        }

        return next;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Store<TState>));
        if (!_isInitialized) throw new InvalidOperationException($"Store for {typeof(TState).FullName} is not initialized.");
    }

    private class Subscription : IDisposable
    {
        public readonly Guid Id;
        private Store<TState>? _store;

        public Subscription(Store<TState> store)
        {
            this._store = store;
            this.Id = Guid.NewGuid();
        }

        public void Dispose()
        {
            var store = Interlocked.Exchange(ref _store, null);
            store?.Unsubscribe(Id);
        }
    }
}
