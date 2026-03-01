namespace Rewind.Extensions.Persistence.Client;

public class LocalStorageState
{
    public string Type { get; set; }
    public string Name { get; set; }
    public long Version { get; set; }
    public string StateJson { get; set; }
    public string? Reason { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LocalStorageState() { }

    public LocalStorageState(string type, string name, long version, string stateJson, string? reason, DateTime updatedAt)
    {
        Type = type;
        Name = name;
        Version = version;
        StateJson = stateJson;
        Reason = reason;
        UpdatedAt = updatedAt;
    }

    public LocalStorageState(PersistenceData data)
    {
        Type = data.Type;
        Name = data.Name;
        Version = data.Version;
        StateJson = data.StateJson;
        Reason = data.Reason;
        UpdatedAt = data.UpdatedAt;
    }

    public PersistenceData ToPersistenceData()
    {
        return new PersistenceData(Type, Name, Version, StateJson, UpdatedAt, Reason);
    }

    public LocalStorageKey ToKey => new LocalStorageKey(Name, Type, Version);
}
