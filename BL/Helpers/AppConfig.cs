namespace Helpers;

/// <summary>
/// Centralized configuration constants to avoid magic numbers throughout the codebase.
/// </summary>
internal static class AppConfig
{
    #region ID Validation

    /// <summary>
    /// Minimum value for a valid Israeli ID (Teudat Zehut).
    /// </summary>
    public const int MinIsraeliId = 100000000;

    /// <summary>
    /// Maximum value for a valid Israeli ID (Teudat Zehut).
    /// </summary>
    public const int MaxIsraeliId = 999999999;

    #endregion

    #region String Length Limits

    /// <summary>
    /// Minimum length for a person's name.
    /// </summary>
    public const int MinNameLength = 2;

    /// <summary>
    /// Maximum length for a person's name.
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Minimum length for an address.
    /// </summary>
    public const int MinAddressLength = 5;

    /// <summary>
    /// Maximum length for an address.
    /// </summary>
    public const int MaxAddressLength = 200;

    /// <summary>
    /// Minimum length for a phone number (digits only).
    /// </summary>
    public const int MinPhoneLength = 9;

    /// <summary>
    /// Maximum length for a phone number (digits only).
    /// </summary>
    public const int MaxPhoneLength = 15;

    #endregion

    #region Distance Limits

    /// <summary>
    /// Maximum reasonable delivery distance in kilometers.
    /// </summary>
    public const double MaxDeliveryDistance = 500.0;

    /// <summary>
    /// Minimum courier max distance in kilometers.
    /// </summary>
    public const double MinCourierMaxDistance = 0.1;

    #endregion

    #region UI Configuration

    /// <summary>
    /// Default window width for detail windows.
    /// </summary>
    public const double DefaultDetailWindowWidth = 600;

    /// <summary>
    /// Default window height for detail windows.
    /// </summary>
    public const double DefaultDetailWindowHeight = 750;

    /// <summary>
    /// Default window width for list windows.
    /// </summary>
    public const double DefaultListWindowWidth = 1100;

    /// <summary>
    /// Default window height for list windows.
    /// </summary>
    public const double DefaultListWindowHeight = 750;

    #endregion

    #region Error Messages

    public const string ErrorInvalidEmail = "Invalid email address format.";
    public const string ErrorInvalidPhone = "Invalid phone number. Must contain 9-15 digits.";
    public const string ErrorInvalidId = "Invalid ID. Must be a 9-digit number.";
    public const string ErrorEmptyName = "Name cannot be empty.";
    public const string ErrorEmptyAddress = "Address cannot be empty.";
    public const string ErrorNegativeDistance = "Distance cannot be negative.";
    public const string ErrorFutureDate = "Date cannot be in the future.";
    public const string ErrorUnauthorized = "You are not authorized to perform this action.";

    #endregion
}
