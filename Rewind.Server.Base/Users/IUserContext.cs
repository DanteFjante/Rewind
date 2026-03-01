using System.Security.Claims;

namespace Rewind.Server.Users
{
    public interface IUserContext
    {
        public string? UserId { get; }
        public bool IsAuthenticated { get; }
        public ClaimsPrincipal? Principal { get; }
    }
}
