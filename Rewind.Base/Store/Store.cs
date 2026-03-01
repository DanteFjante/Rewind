using Rewind.Middleware;
using Rewind.Store;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Rewind.Store;

internal class Store<TState> : IInitializableStore<TState>, IStore<TState>
{
    //State
    public StoreKey Key { get { lock (_gate) return _current.Key; } }
    public long Version => Snapshot.Version;
    public DateTime UpdatedAt => Snapshot.UpdatedAt;
    public string? Reason => Snapshot.Reason;

    public Snapshot<TState> Snapshot { get { lock (_gate) return _current; } }
    private Snapshot<TState> _current;

    //Lifecycle
    public bool IsInitialized => _isInitialized;
    public bool IsDisposed => _disposed;


    private readonly object _gate = new();
    private volatile bool _disposed;
    private volatile bool _isInitialized;

    //Middleware
    private IEnumerable<BaseMiddleware<TState>> _middleware;
    private IEnumerable<Func<BaseMiddleware<TState>>> _middlewareInits;
    private BaseMiddleware<TState>.UpdateNextAsync _updatePipeline;
    private BaseMiddleware<TState>.InitNextAsync _initPipline;

    //Event handling
    private readonly ConcurrentDictionary<Guid, Action<TState>> _dispatchSubscribers;
    private readonly ConcurrentDictionary<Guid, Action<StoreKey>> _disposedSubscibers;
    private readonly ConcurrentDictionary<Guid, Action<IInitializableStore>> _initializedSubscribers;

    public Store(TState initial, StoreKey? key = null, IEnumerable<Func<BaseMiddleware<TState>>>? middlewareInits = null)
    {
        var storeKey = key ?? new StoreKey(typeof(TState).FullName!, "");
        _current = new Snapshot<TState>(storeKey, initial);
        _middlewareInits = middlewareInits ?? new List<Func<BaseMiddleware<TState>>>();
        _middleware = new List<BaseMiddleware<TState>>();

        _dispatchSubscribers = new();
        _disposedSubscibers = new();
        _initializedSubscribers = new();

        _initPipline = null!;
        _updatePipeline = null!;
    }
    public Store(Snapshot<TState> initial, IEnumerable<Func<BaseMiddleware<TState>>>? middlewareInits = null)
    {
        _current = initial;
        _middlewareInits = middlewareInits ?? new List<Func<BaseMiddleware<TState>>>();
        _middleware = new List<BaseMiddleware<TState>>();

        _dispatchSubscribers = new();
        _disposedSubscibers = new();
        _initializedSubscribers = new();

        _initPipline = null!;
        _updatePipeline = null!;
    }

    #region State

    public string GetState()
    {
        return JsonSerializer.Serialize(this.Snapshot.State);
    }

    public SerializableSnapshot GetSnapshot()
    {
        return SerializableSnapshot.FromSnapshot(Snapshot);
    }

    public async ValueTask UpdateAsync(Func<TState, TState> reducer, string? reason = null, CancellationToken ct = default)
    {
        Action<TState>[] listeners;
        TState oldValue;
        StoreKey key;
        long version;

        lock (_gate)
        {
            ThrowIfNotReady();
            oldValue = _current.State;
            key = _current.Key;
            version = _current.Version + 1;
        }

        var context = new UpdateMiddlewareContext<TState>(reducer, oldValue, key, version, reason ?? "");

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
                Reason = context.Reason,
                UpdatedAt = DateTime.UtcNow,
                State = newState,
                Version = context.Version
            };
            listeners = _dispatchSubscribers.Values.ToArray();
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

    public async ValueTask SetState(string serializedState, string reason, CancellationToken ct = default)
    {
        lock (_gate)
        {
            ThrowIfNotReady();
        }

        var newState = JsonSerializer.Deserialize<TState>(serializedState);
        if (newState is TState)
        {
            await UpdateAsync(state => (TState)newState, reason, ct);
            return;
        }
        
        var newSnapshot = JsonSerializer.Deserialize<Snapshot<TState>>(serializedState);
        if (newSnapshot is Snapshot<TState>)
        {
            await SetSnapshot(newSnapshot);
            return;
        }
        
        var serializable = JsonSerializer.Deserialize<SerializableSnapshot>(serializedState);
        if (serializable is SerializableSnapshot)
        {
            await SetSnapshot(serializable.ToSnapshot<TState>());
            return;
        }
        throw new InvalidDataException("Serialized data for set state needs to be eiher StoreState<TState> or TState");

    }

    public ValueTask SetSnapshot(SerializableSnapshot snapshot, bool silent = false, CancellationToken ct = default)
    {
        return SetSnapshot(snapshot.ToSnapshot<TState>(), silent, ct);
    }
    
    public async ValueTask SetSnapshot(Snapshot<TState> snapshot, bool silent = false, CancellationToken ct = default)
    {
        lock (_gate)
        {
            ThrowIfNotReady();
            if (ReferenceEquals(_current, snapshot))
                return;
        }
        if (silent)
        {
            lock (_gate)
            {
                _current = snapshot;
                return;
            }
        }


        Action<TState>[] listeners;
        TState oldValue;
        StoreKey key;
        long version;

        lock (_gate)
        {
            ThrowIfNotReady();
            oldValue = _current.State;
            key = snapshot.Key;
            version = snapshot.Version;
        }

        var context = new UpdateMiddlewareContext<TState>(
            state => snapshot.State,
            oldValue,
            key,
            version, 
            snapshot.Reason ?? ""
            );

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
                Reason = context.Reason,
                UpdatedAt = DateTime.UtcNow,
                State = newState,
                Version = context.Version
            };
            listeners = _dispatchSubscribers.Values.ToArray();
        }

        foreach (var action in listeners)
        {
            try
            {
                action.Invoke(newState);
            }
            catch { }
        }
        return;
    }

    #endregion

    #region Middleware

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

    #endregion

    #region LifeCycle
    public async ValueTask InitializeAsync(CancellationToken ct = default)
    {

        if (_disposed)
            throw new ObjectDisposedException("Can not initialize a disposed store");

        if (_isInitialized)
            return;

        lock (_gate)
        {
            _middleware = _middlewareInits.Select(x => x());

            _initPipline = BuildInitPipeline();
            _updatePipeline = BuildUpdatePipeline();
        }

        InitializeMiddlewareContext<TState> context;
        
        lock (_gate)
        {
            var key = _current.Key;
            var state = _current.State;
            context = new InitializeMiddlewareContext<TState>(key, state);
        }

        await _initPipline(context, ct);

        if (context.Blocked)
        {
            throw new InvalidOperationException($"Could not initialize store [{typeof(TState).FullName}] because of: {context.BlockedReason}");
        }

        lock (_gate)
        {
            _current = _current with
            {
                Reason = context.Reason,
                UpdatedAt = context.At,
                State = context.State,
                Version = context.Version
            };
            _isInitialized = true;
        }
        var subscribers = _initializedSubscribers.Values.AsEnumerable();
        foreach (var init in subscribers)
        {
            init(this);
        }

        _initializedSubscribers.Clear();
    }
    public void Dispose()
    {
        if (_disposed) 
            return;

        _disposed = true;

        _dispatchSubscribers.Clear();
        _initializedSubscribers.Clear();

        var key = Snapshot.Key;
        var subscribers = _disposedSubscibers.Values;
        _disposedSubscibers.Clear();

        foreach (var item in subscribers)
        {
            item.Invoke(key);
        }
    }

    private void ThrowIfNotReady()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Store<TState>));
        if (!_isInitialized) throw new InvalidOperationException($"Store for {typeof(TState).FullName} is not initialized.");
    }

    #endregion

    #region Subsciptions

    public IDisposable Subscribe(Action listener)
    {
        if (_disposed)
            ThrowIfNotReady();

        Subscription subscription = Subscription.OnDispatch(this);
        _dispatchSubscribers.TryAdd(subscription.Id, _ => listener());

        return subscription;
    }

    public IDisposable Subscribe(Action<TState> listener, bool fireImmediately = true)
    {
        if(_disposed)
            ThrowIfNotReady();

        Subscription subscription = Subscription.OnDispatch(this);
        _dispatchSubscribers.TryAdd(subscription.Id, listener);

        if (_isInitialized && fireImmediately)
        {
            TState state;
            lock (_gate)
            {
                state = _current.State;
            }
            try
            {
                listener.Invoke(state);
            }
            catch { }
        }

        return subscription;
    }

    public IDisposable SubscribeOnDisposed(Action<StoreKey> action)
    {
        if(_disposed)
            ThrowIfNotReady();

        var sub = Subscription.OnDisposed(this);
        _disposedSubscibers.TryAdd(sub.Id, action);

        return sub;
    }

    public IDisposable SubscribeOnInitialized(Action<IInitializableStore> action)
    {
        if (_disposed)
            ThrowIfNotReady();

        if (_isInitialized)
            return Subscription.Null;

        var sub = Subscription.OnInitialized(this);
        _initializedSubscribers.TryAdd(sub.Id, action);

        return sub;
    }

    private void Unsubscribe(Guid id, SubcriptionType type)
    {

        if (_disposed) return;

        switch (type)
        {
            case SubcriptionType.OnDispatch:
                _dispatchSubscribers.TryRemove(id, out _);
                break;
            case SubcriptionType.OnDisposed:
                _disposedSubscibers.TryRemove(id, out _);
                break;
            case SubcriptionType.OnInitialized:
                _initializedSubscribers.TryRemove(id, out _);
                break;
        }
    }

    public enum SubcriptionType
    {
        OnDispatch,
        OnInitialized,
        OnDisposed
    }

    private class Subscription : IDisposable
    {
        public readonly Guid Id;
        public readonly SubcriptionType Type;
        private Store<TState>? _store;

        public Subscription(Store<TState> store, SubcriptionType type)
        {
            this._store = store;
            this.Id = Guid.NewGuid();
            this.Type = type;
        }

        private Subscription()
        {
            this._store = null;
            this.Id = Guid.Empty;
            this.Type = SubcriptionType.OnDisposed;
        }

        public static Subscription OnDispatch(Store<TState> store) => new Subscription(store, SubcriptionType.OnDispatch);

        public static Subscription OnInitialized(Store<TState> store) => new Subscription(store, SubcriptionType.OnInitialized);

        public static Subscription OnDisposed(Store<TState> store) => new Subscription(store, SubcriptionType.OnDisposed);
        public static Subscription Null { get; } = new Subscription();

        public void Dispose()
        {
            if (_store == null)
                return;

            var store = Interlocked.Exchange(ref _store, null);
            store?.Unsubscribe(Id, Type);
        }
    }
    #endregion
}
