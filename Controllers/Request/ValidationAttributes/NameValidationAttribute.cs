using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class NameValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is string name && name.Length >= 4 && name.Length <= 10)
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult("Name must be between 4 and 10 characters long.");
        }
    }
}
