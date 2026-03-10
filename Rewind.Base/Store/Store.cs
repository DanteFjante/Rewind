using Rewind.Common;
using Rewind.Middleware;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;

namespace Rewind.Store;

internal class Store<TState> : IInitializableStore<TState>, IStore<TState>
    where TState : class
{

    public ConcurrentDictionary<string, Snapshot<TState>> Snapshots { get; }

    public Snapshot<TState> Snapshot { get { lock (_gate) return Snapshots[""]; } }

    //Lifecycle
    public bool IsInitialized => _isInitialized;
    public bool IsDisposed => _disposed;

    public string Type { get; } = HelperMethods.StoreType<TState>();

    object? IStore.this[string key] => Get(key)?.State;

    public TState? this[string key] => Get(key)?.State;

    private readonly object _gate = new();
    private volatile bool _disposed;
    private volatile bool _isInitialized;
    private Func<TState> _initialState;

    //Middleware
    private IEnumerable<BaseMiddleware<TState>> _middleware;
    private IEnumerable<Func<BaseMiddleware<TState>>> _middlewareInits;
    private BaseMiddleware<TState>.UpdateNextAsync _updatePipeline;
    private BaseMiddleware<TState>.InitNextAsync _initPipline;

    //Event handling
    private readonly ConcurrentDictionary<Guid, Action<StoreKey, TState>> _dispatchSubscribers;
    private readonly ConcurrentDictionary<Guid, Action<StoreKey>> _disposedSubscibers;
    private readonly ConcurrentDictionary<Guid, Action<StoreKey, IInitializableStore>> _initializedSubscribers;

    public Store(Func<TState> initial, IEnumerable<Func<BaseMiddleware<TState>>>? middlewareInits = null)
    {
        _initialState = initial;
        _middlewareInits = middlewareInits ?? new List<Func<BaseMiddleware<TState>>>();
        _middleware = new List<BaseMiddleware<TState>>();

        _dispatchSubscribers = new();
        _disposedSubscibers = new();
        _initializedSubscribers = new();

        _initPipline = null!;
        _updatePipeline = null!;

        Snapshots = new ConcurrentDictionary<string, Snapshot<TState>>();

        Add(initial());
    }


    #region State
    public IEnumerable<string> GetKeys()
    {
        FrozenSet<string> keys;
        lock (_gate)
        {
            keys = Snapshots.Keys.ToFrozenSet();
        }

        return keys;
    }
    public async ValueTask<bool> CreateStateAsync(string key, CancellationToken ct = default)
    {
        if(Add(_initialState(), key))
        {
            if (_isInitialized)
            {
                await InitializeStateAsync(key, ct);
            }
            return true;
        }

        return false;
    }

    public string? GetState(string key = "")
    {
        var snapshot = Get(key);
        if (snapshot == null)
            return null;

        return JsonSerializer.Serialize(snapshot.State);
    }

    public SerializableSnapshot? GetSnapshot(string key = "")
    {
        var snapshot = Get(key);
        if (snapshot == null)
            return null;

        return SerializableSnapshot.FromSnapshot(snapshot);
    }
    Snapshot<TState>? IStore<TState>.GetSnapshot(string name)
    {
        return Get(name);
    }

    public async ValueTask UpdateAsync(Func<TState, TState> reducer, string key = "", string? reason = null, CancellationToken ct = default)
    {
        var current = Get(key);

        if (current == null)
            return;

        Action<StoreKey, TState>[] listeners;
        TState oldValue;
        StoreKey storekey;
        long version;

        lock (_gate)
        {
            ThrowIfNotReady();
            
            oldValue = current.State;
            storekey = current.Key;
            version = current.Version + 1;
        }

        var context = new UpdateMiddlewareContext<TState>(reducer, oldValue, storekey, version, reason ?? "");

        await _updatePipeline(context, ct);

        if (context.Blocked)
            return;

        var newState = context.NextState ?? context.Reducer(oldValue);

        if (ReferenceEquals(oldValue, newState))
            return;

        var n = current with
        {
            Reason = context.Reason,
            UpdatedAt = DateTime.UtcNow,
            State = newState,
            Version = context.Version
        };

        if (!Update(n))
        {
            return;
        }
        

        lock (_gate)
        {
            listeners = _dispatchSubscribers.Values.ToArray();
        }

        foreach (var action in listeners)
        {
            try
            {
                action.Invoke(current.Key, newState);
            }
            catch { }
        }
    }

    public ValueTask SetSnapshot(SerializableSnapshot snapshot, bool silent = false, CancellationToken ct = default)
    {
        return SetSnapshot(snapshot.ToSnapshot<TState>(), silent, ct);
    }

    public async ValueTask SetSnapshot(Snapshot<TState> snapshot, bool silent = false, CancellationToken ct = default)
    {
        var current = Get(snapshot.Key.Name);
        if (current == null)
            return;

        lock (_gate)
        {
            ThrowIfNotReady();
            if (ReferenceEquals(current, snapshot))
                return;
        }
        if (silent)
        {
            Update(snapshot);
            return;
        }

        Action<StoreKey, TState>[] listeners;
        TState oldValue;
        StoreKey storekey;
        long version;

        lock (_gate)
        {
            ThrowIfNotReady();
            oldValue = current.State;
            storekey = snapshot.Key;
            version = snapshot.Version;
        }

        var context = new UpdateMiddlewareContext<TState>(
            state => snapshot.State,
            oldValue,
            storekey,
            version, 
            snapshot.Reason ?? ""
            );

        await _updatePipeline(context, ct);

        if (context.Blocked)
            return;

        var newState = context.NextState ?? context.Reducer(oldValue);

        if (ReferenceEquals(oldValue, newState))
            return;

        var n = current with
        {
            Reason = context.Reason,
            UpdatedAt = DateTime.UtcNow,
            State = newState,
            Version = context.Version
        };
        if (!Update(n))
        {
            return;
        }

        lock (_gate)
        {
            listeners = _dispatchSubscribers.Values.ToArray();
        }

        foreach (var action in listeners)
        {
            try
            {
                action.Invoke(current.Key, newState);
            }
            catch { }
        }
        return;
    }

    public async ValueTask SetState(string serializedState, string key = "", string? reason = null, CancellationToken ct = default)
    {
        lock (_gate)
        {
            ThrowIfNotReady();
        }

        var newState = JsonSerializer.Deserialize<TState>(serializedState);
        if (newState is TState)
        {
            await UpdateAsync(state => (TState)newState, key, reason, ct);
            return;
        }

        throw new InvalidDataException($"Serialized data for set state needs to be of type {HelperMethods.StoreType<TState>()}");
    }

    public async ValueTask SetSnapshot(string serializedSnapshot, string? reason = null, CancellationToken ct = default)
    {
        lock (_gate)
        {
            ThrowIfNotReady();
        }

        var newSnapshot = JsonSerializer.Deserialize<Snapshot<TState>>(serializedSnapshot);
        if (newSnapshot is Snapshot<TState>)
        {
            newSnapshot = newSnapshot with
            {
                Reason = reason ?? newSnapshot.Reason
            };
            await SetSnapshot(newSnapshot);
            return;
        }

        var serializable = JsonSerializer.Deserialize<SerializableSnapshot>(serializedSnapshot);
        if (serializable is SerializableSnapshot)
        {
            serializable = serializable with {
                Reason = reason ?? serializable.Reason
            };

            await SetSnapshot(serializable.ToSnapshot<TState>());
            return;
        }
        throw new InvalidDataException($"Serialized data for set snapshot needs to be eiher SerializableState, StoreState<{HelperMethods.StoreType<TState>()}>");
    }

    #region Private Collection Functions
    private bool Update(TState state, string key = "", string? reason = null)
    {

        if (Snapshots.TryGetValue(key, out var snapshot))
        {
            if (Snapshots.TryUpdate(key, snapshot.Update(state, reason), snapshot))
            {
                return true;
            }
        }

        return false;
    }

    private bool Update(Snapshot<TState> snapshot, string? reason = null)
    {

        if (Snapshots.TryGetValue(snapshot.Key.Name, out var oldSnapshot))
        {
            if (Snapshots.TryUpdate(snapshot.Key.Name, snapshot, oldSnapshot))
            {
                return true;
            }
        }

        return false;
    }

    private bool Add(Snapshot<TState> snapshot)
    {
        return Snapshots.TryAdd(snapshot.Key.Name, snapshot);
    }

    private bool Add(TState state, string key = "")
    {
        var snapshot = new Snapshot<TState>(new StoreKey(HelperMethods.StoreType<TState>(), key), state);
        return Snapshots.TryAdd(key, snapshot);
    }

    private bool Remove(string key)
    {
        bool success = Snapshots.Remove(key, out _);
        return success;
    }

    private Snapshot<TState>? Get(string key)
    {
        if (Snapshots.TryGetValue(key, out var snapshot))
        {
            return snapshot;
        }
        return null;
    }

    #endregion

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

        bool allSuccess = true;
        foreach (var snapshot in Snapshots)
        {
            if (!await InitializeStateAsync("", ct))
                allSuccess = false;
        }

        _isInitialized = allSuccess;

    }

    private async Task<bool> InitializeStateAsync(string key, CancellationToken ct = default)
    {

        InitializeMiddlewareContext<TState> context;
        var current = Get(key);

        if (current == null)
            return false;

        lock (_gate)
        {
            var storekey = current.Key;
            var state = current.State;
            context = new InitializeMiddlewareContext<TState>(storekey, state);
        }

        await _initPipline(context, ct);

        if (context.Blocked)
        {
            throw new InvalidOperationException($"Could not initialize store [{HelperMethods.StoreType<TState>()}] because of: {context.BlockedReason}");
        }

        lock (_gate)
        {
            var success = Update(current with
            {
                Reason = context.Reason,
                UpdatedAt = context.At,
                State = context.State,
                Version = context.Version
            });
            if (!success) return false;
        }

        var subscribers = _initializedSubscribers.Values.AsEnumerable();
        foreach (var init in subscribers)
        {
            init(current.Key, this);
        }

        return true;
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
        if (!_isInitialized) throw new InvalidOperationException($"Store for {HelperMethods.StoreType<TState>()} is not initialized.");
    }

    #endregion

    #region Subsciptions

    public IDisposable Subscribe(Action<StoreKey> listener)
    {
        if (_disposed)
            ThrowIfNotReady();

        Subscription subscription = Subscription.OnDispatch(this);
        _dispatchSubscribers.TryAdd(subscription.Id, (key,_) => listener(key));

        return subscription;
    }

    public IDisposable Subscribe(Action<StoreKey, TState> listener)
    {
        if(_disposed)
            ThrowIfNotReady();

        Subscription subscription = Subscription.OnDispatch(this);
        _dispatchSubscribers.TryAdd(subscription.Id, listener);

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

    public IDisposable SubscribeOnInitialized(Action<StoreKey, IInitializableStore> action)
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
