namespace Rewind.Extensions.Users
{
    public class UserService
    {
        public string UserId { get; set; }

        public UserService(string userId)
        {
            UserId = userId;
        }

        public UserService()
        {
            UserId = Guid.NewGuid().ToString();
        }
    }
}
