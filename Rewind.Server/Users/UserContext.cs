using System.Security.Claims;

namespace Rewind.Server.Users
{
    public sealed class UserContext : IUserContext
    {
        public string? UserId { get; internal set; }
        public bool IsAuthenticated { get; internal set; }
        public ClaimsPrincipal? Principal { get; internal set; }
    }
}
