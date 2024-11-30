using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.ValidationAttributes.RequiredAtributes
{
    public class RequiredIfMissingAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;

        public RequiredIfMissingAttribute(string dependentProperty)
        {
            _dependentProperty = dependentProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dependentProperty = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (dependentProperty == null)
            {
                return new ValidationResult($"Property '{_dependentProperty}' does not exist.");
            }

            var dependentValue = dependentProperty.GetValue(validationContext.ObjectInstance);

            if (dependentValue == null && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required because '{_dependentProperty}' is missing.");
            }

            return ValidationResult.Success;
        }
    }
}
