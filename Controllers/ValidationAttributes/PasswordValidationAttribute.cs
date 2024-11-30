using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class PasswordValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string password 
                && password.Length >= 8 
                && password.Length <= 20 
                && HasAtLeastOneUppercaseLetter(password) 
                && HasAtLeastOneLowercaseLetter(password) 
                && HasAtLeastOneDigit(password) 
                && HasAtLeastOneSpecialCharacter(password))
            {
                return ValidationResult.Success!;
            }

            return new ValidationResult(
                    "Password must be between 8 and 20 characters long and contain at least one uppercase letter, " +
                    "one lowercase letter, one digit, and one special character."
                );
        }

        private static bool HasAtLeastOneUppercaseLetter(string password) => password.Any(char.IsUpper);
        private static bool HasAtLeastOneLowercaseLetter(string password) => password.Any(char.IsLower);
        private static bool HasAtLeastOneDigit(string password) => password.Any(char.IsDigit);
        private static bool HasAtLeastOneSpecialCharacter(string password) => password.Any(c => !char.IsLetterOrDigit(c));
    }
}

