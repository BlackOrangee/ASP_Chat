using ASP_Chat.Controllers.Request.ValidationAttributes;

namespace ASP_Chat.Controllers.Request
{
    public class UserUpdateRequest
    {
        [NameValidation]
        public string? Name { get; set; }

        [UniqueNameValidation]
        public string? Username { get; set; }

        public string? Description { get; set; }
    }
}
