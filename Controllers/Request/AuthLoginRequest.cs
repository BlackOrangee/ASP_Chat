using System.ComponentModel.DataAnnotations;
using ASP_Chat.Controllers.Request.ValidationAttributes;

namespace ASP_Chat.Controllers.Request
{
    public class AuthLoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [UniqueNameValidation]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
