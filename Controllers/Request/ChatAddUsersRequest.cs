using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request
{
    public class ChatAddUsersRequest
    {
        [Required(ErrorMessage = "Users are required")]
        public ICollection<long> Users { get; set; }
    }
}
