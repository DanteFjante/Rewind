namespace Rewind.Extensions.Users
{
    public record class LoginResponse(string token, DateTime expiry);
    public record class LoginRequest(string userName, string passKey);

    public record class UserCredentials(string UserName, string Password);
    public record class UserLogin(string UserName, string Token, DateTime ExpiresAt);

}
