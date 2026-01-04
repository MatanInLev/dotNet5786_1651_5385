using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PL.ValidationRules
{
    /// <summary>
    /// Validates email address format in XAML bindings.
    /// </summary>
    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? email = value as string;

            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationResult.ValidResult;
            }

            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                if (!emailRegex.IsMatch(email))
                {
                    return new ValidationResult(false, "Invalid email format. Expected format: user@domain.com");
                }

                return ValidationResult.ValidResult;
            }
            catch
            {
                return new ValidationResult(false, "Invalid email format.");
            }
        }
    }
}
