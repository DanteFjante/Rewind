using Rewind.Store;
using System.Text.Json;

namespace Rewind.Extensions.Persistence;

public record class PersistenceKey(string Type, string Name = "")
{
    public PersistenceKey(StoreKey key) : this(key.Type, key.Name)
    {

    }

    public StoreKey StoreKey => new StoreKey(Type, Name);

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public static PersistenceKey? FromString(string data)
    {
        return JsonSerializer.Deserialize<PersistenceKey>(data);
    }

}
