using System.ComponentModel.DataAnnotations;

namespace TaskManagementPlatform.Models.ValidationAttributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var dateValue = (DateTime)value;
            if (dateValue < DateTime.Now)
            {
                return new ValidationResult(ErrorMessage ?? "The date must be in the future.");
            }

            return ValidationResult.Success;
        }
    }
}
