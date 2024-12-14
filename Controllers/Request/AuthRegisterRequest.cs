using System.ComponentModel.DataAnnotations;
using ASP_Chat.Controllers.Request.ValidationAttributes;

namespace ASP_Chat.Controllers.Request
{
    public class AuthRegisterRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [NameValidation]
        public string Name { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [UniqueNameValidation]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [PasswordValidation]
        public string Password { get; set; }
    }
}
