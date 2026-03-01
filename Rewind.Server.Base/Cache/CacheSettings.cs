namespace Rewind.Server.Base.Cache
{
    public class CacheSettings
    {
        public TimeSpan StateLifetime { get; set; } = new TimeSpan(0, 15, 0);
    }
}
