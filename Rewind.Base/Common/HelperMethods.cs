namespace Rewind.Common
{
    public static class HelperMethods
    {
        public static string StoreType<TState>()
        {
            return typeof(TState).FullName!;
        }
    }
}
