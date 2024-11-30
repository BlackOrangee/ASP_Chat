using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request.ValidationAttributes
{
    public class NotEqualToAttribute : ValidationAttribute
    {
        private readonly string _compareToField;

        public NotEqualToAttribute(string compareToField)
        {
            _compareToField = compareToField;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var compareToProperty = validationContext.ObjectType.GetProperty(_compareToField);
            if (compareToProperty == null)
            {
                return new ValidationResult($"Property '{_compareToField}' does not exist.");
            }

            var compareToValue = compareToProperty.GetValue(validationContext.ObjectInstance)?.ToString();
            var currentValue = value?.ToString();

            if (compareToValue == currentValue)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must not be the same as {_compareToField}.");
            }

            return ValidationResult.Success;
        }
    }
}
