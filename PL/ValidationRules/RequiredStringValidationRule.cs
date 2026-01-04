using System.Globalization;
using System.Windows.Controls;

namespace PL.ValidationRules
{
    /// <summary>
    /// Validates that a string is not empty or whitespace in XAML bindings.
    /// </summary>
    public class RequiredStringValidationRule : ValidationRule
    {
        public string FieldName { get; set; } = "Field";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? text = value as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                return new ValidationResult(false, $"{FieldName} is required.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
