using ASP_Chat.Controllers.Request.ValidationAttributes;
using ASP_Chat.Controllers.Request.ValidationAttributes.RequiredAtributes;
using ASP_Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request
{
    public class ChatCreateRequest
    {
        [Required (ErrorMessage = "TypeId is required")]
        public long TypeId { get; set; }

        [Required (ErrorMessage = "Users is required")]
        public ICollection<long> Users { get; set; }

        [RequiredIfFieldValueBetwinAttribute(nameof(TypeId), (long)ChatTypes.Channel, ErrorMessage = "Tag is required")]
        [UsernameValidation]
        public string? Tag { get; set; }

        [RequiredIfFieldValueBetwinAttribute(nameof(TypeId), (long)ChatTypes.Group, (long)ChatTypes.Channel, ErrorMessage = "Name is required")]
        [NameValidation]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
