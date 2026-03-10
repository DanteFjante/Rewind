using Rewind.Settings;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Rewind.Extensions.Users
{
    public class UserService : ITokenProvider, IDisposable
    {
        public UserLogin? Login { get; set; }
        public UserCredentials? Credentials { get; set; }
        private HttpClient client { get; }

        public UserService(UserLogin login, SyncSettings settings, HttpClient client) 
            : this(settings, client)
        {
            Login = login;
        }

        public UserService(SyncSettings settings, HttpClient client)
        {
            this.client = client;
            client.BaseAddress =
                new UriBuilder(settings.ServerProtocol, settings.ServerAdress, settings.ServerPort).Uri;
        }

        public async ValueTask<UserLogin?> LoginUser(
            UserCredentials credentials, bool force = false, CancellationToken ct = default)
        {

            if (ValidateLogin() && !force)
            {
                if (Login!.UserName == credentials.UserName)
                {
                    return Login;
                }
            }
            if (credentials.Password.Length < 4)
            {
                credentials = credentials with { Password = credentials.Password + "0000"};
            }

            var passBytes = Convert.FromBase64String(credentials.Password);
            var hash = SHA256.HashData(passBytes);
            //var passKeyBytes = Shake256.HashData(passBytes, 32);
            string passKey = Convert.ToBase64String(hash);

            LoginRequest request = new LoginRequest(credentials.UserName, passKey);
            
            var severResponse = await client.PostAsJsonAsync("/auth/login", request);

            LoginResponse? response = await severResponse.Content.ReadFromJsonAsync<LoginResponse>();

            if (response != null)
            {
                Login = new UserLogin(credentials.UserName, response.token, response.expiry);
                return Login;
            }

            return null;
        }

        public async ValueTask<UserLogin?> LoginUser(bool force = false, CancellationToken ct = default)
        {
            if (ValidateLogin() && !force)
            {
                return Login;
            }
            if (Credentials == null)
                return null;

            var passBytes = Convert.FromBase64String(Credentials.Password);
            //var passKeyBytes = Shake256.HashData(passBytes, 32);
            string passKey = Convert.ToBase64String(passBytes);

            LoginRequest request = new LoginRequest(Credentials.UserName, passKey);

            var severResponse = await client.PostAsJsonAsync("/auth/login", request);

            LoginResponse? response = await severResponse.Content.ReadFromJsonAsync<LoginResponse>();

            if (response != null)
            {
                Login = new UserLogin(Credentials.UserName, response.token, response.expiry);
                return Login;
            }

            return null;
        }

        public bool ValidateLogin()
        {
            if (Login != null)
                return Login.ExpiresAt > DateTime.UtcNow;
            
            return false;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
