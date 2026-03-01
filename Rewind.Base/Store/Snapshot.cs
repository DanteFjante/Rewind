using System.Text.Json;

namespace Rewind.Store;
public record Snapshot<TState>(
    StoreKey Key,
    TState State,
    long Version,
    DateTime UpdatedAt,
    string? Reason)
{
    public Snapshot(StoreKey key, TState State) : this(key, State, 0, DateTime.UtcNow, "Created") {}

    public Snapshot<TState> Update(TState newState, string? reason) => this with
    {
        State = newState,
        Version = Version + 1,
        UpdatedAt = DateTime.UtcNow,
        Reason = reason
    };

    public SerializableSnapshot ToSerializableSnapshot() => SerializableSnapshot.FromSnapshot(this);
}

public record SerializableSnapshot(
    StoreKey Key,
    string State,
    long Version,
    DateTime UpdatedAt,
    string? Reason)
{
    public Snapshot<TState> ToSnapshot<TState>()
    {
        TState? state = JsonSerializer.Deserialize<TState>(State);
        if (state == null)
            throw new InvalidDataException($"Could not deserialize state to {typeof(TState)}");

        return new Snapshot<TState>(Key, state, Version, UpdatedAt, Reason);
    }

    public static SerializableSnapshot FromSnapshot<TState>(Snapshot<TState> snapshot)
    {
        return new SerializableSnapshot(
            snapshot.Key,
            JsonSerializer.Serialize(snapshot.State), 
            snapshot.Version,
            snapshot.UpdatedAt,
            snapshot.Reason
            );
    }

    public TState GetState<TState>()
    {
        TState? state = JsonSerializer.Deserialize<TState>(State);
        if(state == null)
            throw new InvalidDataException($"Could not deserialize state to {typeof(TState)}");

        return state;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public static SerializableSnapshot? FromString(string snapshot)
    {
        return JsonSerializer.Deserialize<SerializableSnapshot>(snapshot);
    }
}
