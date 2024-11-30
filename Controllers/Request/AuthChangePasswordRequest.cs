using System.ComponentModel.DataAnnotations;
using ASP_Chat.Controllers.ValidationAttributes;

namespace ASP_Chat.Controllers.Request
{
    public class AuthChangePasswordRequest
    {
        [Required(ErrorMessage = "Password is required")]
        [PasswordValidation]
        public string Password { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [PasswordValidation]
        public string NewPassword { get; set; }
    }
}
