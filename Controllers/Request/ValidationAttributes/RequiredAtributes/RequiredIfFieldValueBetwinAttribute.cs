using System.ComponentModel.DataAnnotations;

namespace ASP_Chat.Controllers.Request.ValidationAttributes.RequiredAtributes
{
    public class RequiredIfFieldValueBetwinAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly double _minValue;
        private readonly double _maxValue;

        public RequiredIfFieldValueBetwinAttribute(string dependentProperty, double minValue, double maxValue)
        {
            _dependentProperty = dependentProperty;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public RequiredIfFieldValueBetwinAttribute(string dependentProperty, double value)
        {
            _dependentProperty = dependentProperty;
            _minValue = value;
            _maxValue = value;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dependentPropertyInfo = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (dependentPropertyInfo == null)
            {
                return new ValidationResult($"Property '{_dependentProperty}' not found.");
            }

            var dependentValue = dependentPropertyInfo.GetValue(validationContext.ObjectInstance);

            if (dependentValue is IConvertible convertible)
            {
                try
                {
                    var numericValue = Convert.ToDouble(convertible);

                    if (numericValue >= _minValue && numericValue <= _maxValue && value == null)
                    {
                        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
                    }
                }
                catch
                {
                    return new ValidationResult($"Property '{_dependentProperty}' must be numeric.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
