using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class UsernameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string username && username.Length >= 4 && username.Length <= 10 && !username.Contains(" "))
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult("Username must be between 4 and 10 characters long. It must not contain spaces.");
        }
    }
}
