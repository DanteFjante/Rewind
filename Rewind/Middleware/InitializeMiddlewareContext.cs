namespace Rewind.Middleware
{
    public class InitializeMiddlewareContext<TState>
    {
        public TState State { get; set; }

        public string? BlockedReason { get; private set; }

        public bool Blocked { get; private set; }

        public void Block(string reason)
        {
            Blocked = true;
            BlockedReason = reason;
        }

        public InitializeMiddlewareContext(TState state)
        {
            State = state;
        }
    }
}
