// Module Courier.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Based on the project specification PDFs, generate the 'DO.Courier' entity as a 
/// C# record.
/// - Ensure all properties match the 'ישות נתונים שליח' table.
/// - Use nullable types where specified (e.g., MaxDistance).
/// - Omit the 'Password' property as it is an optional add-on.
/// - Include a parameterless constructor for XML serialization.
/// - All documentation comments (///) must be in English."
/// </aiPrompt>
namespace DO;

/// <summary>
/// Represents a courier (delivery person) record in the data-access layer.
/// </summary>
/// <param name="Id">Unique running identifier for the courier. Required. Positive integer.</param>
/// <param name="Name">Courier’s full name. Required for display and matching.</param>
/// <param name="PhoneNumber">Courier’s contact phone number. E.164 recommended format.</param>
/// <param name="Email">Courier’s email address. Optional but recommended for notifications.</param>
/// <param name="Password">Optional login password — intentionally omitted from this DO model (handled in auth/store-specific models).</param>
/// <param name="Distance">Optional maximum or current travel distance/range in kilometers. Unit: km.</param>
/// <param name="IsActive">Indicates whether the courier is active and available for deliveries.</param>
/// <param name="Date">Timestamp when the record was read or generated. Returns current system time by design.</param>
/// <param name="VehicleType">Courier’s primary vehicle type (Bike, Car, Foot, Motorcycle).</param>
/// <remarks>
/// Validation guidance (conventions, not enforced by this POCO):
/// - <see cref="Id"/> should be > 0.
/// - <see cref="PhoneNumber"/> should be a valid phone number (prefer E.164).
/// - <see cref="Distance"/> is measured in kilometers and may be null if not specified.
/// - Prefer storing and transmitting UTC timestamps; <see cref="Date"/> returns local time by default.
/// </remarks>
/// <example>
/// var courier = new Courier
/// {
///     Id = 7,
///     Name = "Alex Rider",
///     PhoneNumber = "+15551234567",
///     Email = "alex@example.com",
///     Distance = 30.0, // kilometers
///     IsActive = true,
///     VehicleType = VehicleType.Bike
/// };
/// // Note: courier.Date returns DateTime.Now at access time; prefer DateTime.UtcNow externally.
/// </example>
/// <seealso cref="VehicleType"/>
public record Courier
{
    /// <summary>
    /// Unique running identifier for the courier. Required. Should be a positive integer.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Courier’s full name used for display and matching.
    /// Defaults to "Unknown" when not provided.
    /// </summary>
    public string Name { get; set; } = "Unknown";

    /// <summary>
    /// Courier’s contact phone number. Prefer E.164 format (e.g., +15551234567).
    /// Defaults to "Unknown" when not provided.
    /// </summary>
    public string PhoneNumber { get; set; } = "Unknown";

    /// <summary>
    /// Courier’s email address. Optional but recommended for notifications.
    /// Defaults to a placeholder value when not provided.
    /// </summary>
    public string Email { get; set; } = "@gmail.com";

    /// <summary>
    /// Optional travel distance or maximum operating range in kilometers (km).
    /// Null when not specified.
    /// </summary>
    public double? Distance { get; set; } = null;

    /// <summary>
    /// Indicates whether the courier is active and available for assignments.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp representing when the record was accessed or created.
    /// This property retrieves the current system time on each access.
    /// Prefer using UTC externally (<see cref="DateTime.UtcNow"/>).
    /// </summary>
    public DateTime Date => DateTime.Now;

    /// <summary>
    /// Primary vehicle type used by the courier.
    /// Defaults to <see cref="VehicleType.Bike"/>.
    /// </summary>
    public VehicleType VehicleType { get; set; } = VehicleType.Bike;

    /// <summary>
    /// Default parameterless constructor. Use object initializers to populate required properties.
    /// </summary>
    public Courier() { }
}
