using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class UniqueNameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is string username && username.Length >= 4 && username.Length <= 15 && !username.Contains(" "))
            {
                return ValidationResult.Success!;
            }

            var displayName = validationContext.DisplayName;
            return new ValidationResult($"{displayName} must be between 4 and 15 characters long. It must not contain spaces.");
        }
    }
}
