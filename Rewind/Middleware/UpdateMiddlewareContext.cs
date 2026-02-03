namespace Rewind.Middleware
{
    public class UpdateMiddlewareContext<TState>
    {
        public Func<TState, TState> Reducer { get; set; }
        public DateTime At { get; }
        public bool Blocked { get; private set; }
        public string? BlockedReason { get; private set; }
        public string Reason { get; set; }

        public TState CurrentState { get; set; }
        public TState? NextState { get; set; }

        public UpdateMiddlewareContext(Func<TState, TState> reducer, TState state, string reason)
        {
            Reducer = reducer;
            CurrentState = state;
            Reason = reason;
        }

        public void Block(string reason)
        {
            Blocked = true;
            BlockedReason = reason;
        }
    }
}
