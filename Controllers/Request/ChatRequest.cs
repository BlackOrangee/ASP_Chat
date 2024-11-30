using ASP_Chat.Enums;
using ASP_Chat.Exceptions;

namespace ASP_Chat.Controllers.Request
{
    public class ChatRequest
    {
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public long? TypeId { get; set; }
        public ICollection<long>? Users { get; set; }
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public void CreateValidation()
        {
            if (Users == null || Users.Count == 0)
            {
                throw ServerExceptionFactory.FeldAreRequired("users list cannot be empty");
            }

            if (TypeId == null || TypeId < 0)
            {
                throw ServerExceptionFactory.FeldAreRequired("typeId");
            }

            if (TypeId > 3)
            {
                throw ServerExceptionFactory.InvalidInput("typeId must be between 0 and 3");
            }

            if (TypeId != (long)ChatTypes.P2P 
                && (string.IsNullOrWhiteSpace(Name) || Name.Length < 3 || Name.Length > 10))
            {
                throw ServerExceptionFactory.InvalidInput("Name must be between 3 and 10 characters long.");
            }

            if (TypeId == (long)ChatTypes.Channel 
                && (string.IsNullOrWhiteSpace(Tag) || Tag.Length < 3 || Tag.Length > 10))
            {
                throw ServerExceptionFactory.InvalidInput("Tag must be between 3 and 10 characters long.");
            }
        }

        public void AddUserValidation()
        {
            if (Users == null || Users.Count == 0)
            {
                throw ServerExceptionFactory.FeldAreRequired("users list cannot be empty");
            }
        }


    }
}
