using ASP_Chat.Entity;

namespace ASP_Chat.Controllers.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public User User { get; set; }

        public static LoginResponse Create(string token, User user)
        {
            return new LoginResponse
            {
                Token = token,
                User = user
            };
        }
    }
}
