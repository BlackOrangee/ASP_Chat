using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request
{
    public class MessageEditRequest
    {
        [Required(ErrorMessage = "Text is required")]
        public string Text { get; set; }
    }
}
