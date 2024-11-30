using ASP_Chat.Exceptions;

namespace ASP_Chat.Controllers.Request
{
    public class UserRequest
    {
        public long UserId { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Description { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description))
            {
                throw ServerExceptionFactory.InvalidInput("You must enter at least one field");
            }

            if (!string.IsNullOrEmpty(Username) && Username.Length < 3 && Username.Length > 10)
            {
                if (Username.Contains(" "))
                {
                    throw ServerExceptionFactory.InvalidInput("Username must not contain spaces.");
                }
                throw ServerExceptionFactory.InvalidInput("Username must be between 3 and 10 characters long.");
            }

            if (!string.IsNullOrEmpty(Name) && Name.Length < 3 && Name.Length > 10)
            {
                throw ServerExceptionFactory.InvalidInput("Name must be between 3 and 10 characters long.");
            }

            if (!string.IsNullOrEmpty(Description) && Description.Length < 3 && Description.Length > 100)
            {
                throw ServerExceptionFactory.InvalidInput("Description must be between 3 and 100 characters long.");
            }
        }
    }
}
