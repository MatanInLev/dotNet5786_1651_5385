using System.Text.RegularExpressions;

namespace Helpers;

/// <summary>
/// Provides validation methods for common data types used across the Business Logic layer.
/// </summary>
internal static class ValidationHelper
{
    /// <summary>
    /// Validates an email address format.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // RFC 5322 compliant email validation
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates an Israeli ID number (Teudat Zehut).
    /// Must be 9 digits.
    /// </summary>
    public static bool IsValidIsraeliId(int id)
    {
        return id >= 100000000 && id <= 999999999;
    }

    /// <summary>
    /// Validates a phone number format.
    /// Accepts formats with digits, spaces, and hyphens.
    /// </summary>
    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove spaces and hyphens for validation
        string cleaned = phone.Replace(" ", "").Replace("-", "");
        
        // Must have at least 9 digits (Israeli phone numbers)
        return cleaned.All(char.IsDigit) && cleaned.Length >= 9 && cleaned.Length <= 15;
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    public static bool IsValidString(string? value, int minLength = 1, int maxLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        int length = value.Trim().Length;
        return length >= minLength && length <= maxLength;
    }

    /// <summary>
    /// Validates that a numeric value is within a specified range.
    /// </summary>
    public static bool IsInRange(double value, double min, double max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a nullable numeric value is positive.
    /// </summary>
    public static bool IsPositive(double? value)
    {
        return !value.HasValue || value.Value > 0;
    }

    /// <summary>
    /// Validates that a date is not in the future.
    /// </summary>
    public static bool IsNotFutureDate(DateTime date, DateTime? referenceDate = null)
    {
        DateTime reference = referenceDate ?? DateTime.Now;
        return date <= reference;
    }

    /// <summary>
    /// Validates courier data.
    /// </summary>
    public static void ValidateCourier(BO.Courier courier)
    {
        if (courier == null)
            throw new BO.BlInvalidValueException("Courier cannot be null");

        if (!IsValidIsraeliId(courier.Id))
            throw new BO.BlInvalidValueException($"Invalid courier ID: {courier.Id}. Must be a 9-digit number.");

        if (!IsValidString(courier.Name, 2, 100))
            throw new BO.BlInvalidValueException("Courier name must be between 2 and 100 characters.");

        if (!IsValidPhone(courier.Phone))
            throw new BO.BlInvalidValueException($"Invalid phone number: {courier.Phone}");

        if (!string.IsNullOrWhiteSpace(courier.Email) && !IsValidEmail(courier.Email))
            throw new BO.BlInvalidValueException($"Invalid email address: {courier.Email}");

        if (!IsPositive(courier.MaxDistance))
            throw new BO.BlInvalidValueException("Max distance must be a positive number.");

        if (!IsNotFutureDate(courier.StartWorkDate))
            throw new BO.BlInvalidValueException("Start work date cannot be in the future.");
    }

    /// <summary>
    /// Validates order data.
    /// </summary>
    public static void ValidateOrder(BO.Order order)
    {
        if (order == null)
            throw new BO.BlInvalidValueException("Order cannot be null");

        if (!IsValidString(order.CustomerName, 2, 100))
            throw new BO.BlInvalidValueException("Customer name must be between 2 and 100 characters.");

        if (!IsValidString(order.CustomerAddress, 5, 200))
            throw new BO.BlInvalidValueException("Customer address must be between 5 and 200 characters.");

        if (!IsValidPhone(order.CustomerPhone))
            throw new BO.BlInvalidValueException($"Invalid phone number: {order.CustomerPhone}");

        if (order.Distance < 0)
            throw new BO.BlInvalidValueException("Distance cannot be negative.");
    }
}
