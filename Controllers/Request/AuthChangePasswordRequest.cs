using ASP_Chat.Controllers.Request.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

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
