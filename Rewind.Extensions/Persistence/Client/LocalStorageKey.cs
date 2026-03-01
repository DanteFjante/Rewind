namespace Rewind.Extensions.Persistence.Client;

public class LocalStorageKey
{
    public string Name { get; set; }
    public string Type { get; set; }
    public long Version { get; set; }

    public LocalStorageKey() { }

    public LocalStorageKey(string name, string type, long version)
    {
        Name = name;
        Type = type;
        Version = version;
    }

    public LocalStorageKey(PersistenceKey key, long version)
    {
        Name = key.Name;
        Type = key.Type;
        Version = version;
    }

    public PersistenceKey ToPersistenceKey() => new PersistenceKey(Name, Type);
}
