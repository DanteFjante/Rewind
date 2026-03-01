using Rewind.Common;
using System.Text.Json;

namespace Rewind.Store
{
    public record struct StoreKey(string Type, string Name = "")
    {
        public static StoreKey Null => new StoreKey("");

        public static StoreKey Create<TState>() => new StoreKey(HelperMethods.StoreType<TState>());

        public override string ToString() => JsonSerializer.Serialize(this);
        public static StoreKey FromString(string name) => JsonSerializer.Deserialize<StoreKey>(name);

    }
}
