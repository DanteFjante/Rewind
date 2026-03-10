namespace Rewind.Extensions.Users
{
    public interface ITokenProvider
    {
        ValueTask<UserLogin?> LoginUser(UserCredentials login, bool force = false, CancellationToken ct = default);
    }
}
