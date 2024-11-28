using ASP_Chat.Exceptions;

namespace ASP_Chat.Controllers.Request
{
    public class AuthRequest
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }

        public void RegisterValidate()
        {
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Username.Length > 10)
            {
                throw ServerExceptionFactory.InvalidInput("Username must be between 3 and 10 characters long.");
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8 || Password.Length > 20)
            {
                throw ServerExceptionFactory.InvalidInput("Password must be between 8 and 20 characters long.");
            }

            if (string.IsNullOrWhiteSpace(Name) || Name.Length < 3 || Name.Length > 10)
            {
                throw ServerExceptionFactory.InvalidInput("Name must be between 3 and 10 characters long.");
            }
        }

        public void LoginValidate()
        {
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Username.Length > 10)
            {
                throw ServerExceptionFactory.InvalidInput("Username must be between 3 and 10 characters long.");
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8 || Password.Length > 20)
            {
                throw ServerExceptionFactory.InvalidInput("Password must be between 8 and 20 characters long.");
            }
        }

        public void ChangePasswordValidate()
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8 || Password.Length > 20)
            {
                throw ServerExceptionFactory.InvalidInput("Password must be between 8 and 20 characters long.");
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8 || NewPassword.Length > 20)
            {
                throw ServerExceptionFactory.InvalidInput("Password must be between 8 and 20 characters long.");
            }

            if (Password == NewPassword)
            {
                throw ServerExceptionFactory.InvalidInput("New password cannot be the same as the old password.");
            }
        }
    }
}
