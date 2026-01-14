using System.ComponentModel.DataAnnotations;

namespace TaskManagementPlatform.Models.ValidationAttributes
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var currentValue = (DateTime)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}");
            }

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue.HasValue && currentValue <= comparisonValue.Value)
            {
                return new ValidationResult(ErrorMessage ?? $"Date must be greater than {_comparisonProperty}");
            }

            return ValidationResult.Success;
        }
    }
}
