using System.Globalization;
using System.Windows.Controls;

namespace PL.ValidationRules
{
    /// <summary>
    /// Validates phone number format in XAML bindings.
    /// </summary>
    public class PhoneValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? phone = value as string;

            if (string.IsNullOrWhiteSpace(phone))
            {
                return new ValidationResult(false, "Phone number is required.");
            }

            // Remove spaces and hyphens for validation
            string cleaned = phone.Replace(" ", "").Replace("-", "");

            // Must have at least 9 digits (Israeli phone numbers)
            if (!cleaned.All(char.IsDigit))
            {
                return new ValidationResult(false, "Phone number must contain only digits, spaces, and hyphens.");
            }

            if (cleaned.Length < 9 || cleaned.Length > 15)
            {
                return new ValidationResult(false, "Phone number must be between 9 and 15 digits.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
