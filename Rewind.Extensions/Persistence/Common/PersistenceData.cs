using Rewind.Common;
using Rewind.Store;
using System.Text.Json;

namespace Rewind.Extensions.Persistence;

public class PersistenceData
{
    public string Type { get; set; }
    public string Name { get; set; }
    public long Version { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? Reason { get; set; }

    public string StateJson { get; set; }

    public TState Get<TState>() => JsonSerializer.Deserialize<TState>(StateJson) ?? throw new InvalidCastException($"Can not cast state to type [{HelperMethods.StoreType<TState>()}]");

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public PersistenceData()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {

    }

    public PersistenceData(string type, string name, long version, object state, DateTime updatedAt, string? reason = null)
    {
        Type = type;
        Name = name;
        Version = version;

        if (state is string)
            StateJson = (string) state;
        else
            StateJson = JsonSerializer.Serialize(state);
        UpdatedAt = updatedAt;
        Reason = reason;
    }

    public PersistenceData(string type, string name, long version, string stateJson, DateTime updatedAt, string? reason = null)
    {
        Type = type;
        Name = name;
        Version = version;
        StateJson = stateJson;
        UpdatedAt = updatedAt;
        Reason = reason;
    }

    public PersistenceData(PersistenceData old, long version, string? reason = null)
    {
        Type = old.Type;
        Name = old.Name;
        Version = version;
        StateJson = old.StateJson;
        UpdatedAt = DateTime.UtcNow;
        Reason = reason;
    }


    public PersistenceData(SerializableSnapshot snapshot)
    {
        Type = snapshot.Key.Type;
        Name = snapshot.Key.Name;
        Version = snapshot.Version;
        UpdatedAt = snapshot.UpdatedAt;
        Reason = snapshot.Reason;
        StateJson = snapshot.State;
    }

    public SerializableSnapshot ToSnapshot() => 
        new SerializableSnapshot(new StoreKey(Type, Name), StateJson, Version, UpdatedAt, Reason);

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public static PersistenceData? FromString(string data)
    {
        return JsonSerializer.Deserialize<PersistenceData>(data);
    }

    public PersistenceKey ToKey() => new PersistenceKey(Type, Name);
}
