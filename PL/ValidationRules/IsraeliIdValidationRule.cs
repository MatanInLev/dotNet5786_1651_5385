using System.Globalization;
using System.Windows.Controls;

namespace PL.ValidationRules
{
    /// <summary>
    /// Validates Israeli ID (Teudat Zehut) format in XAML bindings.
    /// </summary>
    public class IsraeliIdValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "ID is required.");
            }

            string? idStr = value.ToString();
            if (string.IsNullOrWhiteSpace(idStr))
            {
                return new ValidationResult(false, "ID is required.");
            }

            if (!int.TryParse(idStr, out int id))
            {
                return new ValidationResult(false, "ID must be a number.");
            }

            if (id < 100000000 || id > 999999999)
            {
                return new ValidationResult(false, "ID must be a 9-digit number.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
