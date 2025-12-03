// Module Order.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Based on the project specification PDFs (General + Stage 1), generate the 'DO.Order'
/// entity as a C# record. 
/// - Ensure all properties match the 'ישות נתונים הזמנה' table.
/// - Use nullable types where specified (e.g., Description, Volume).
/// - Include a parameterless constructor for XML serialization, as required by Stage 3.
/// - All documentation comments (///) must be in English."
/// </aiPrompt>
namespace DO;

/// <summary>
/// Represents an order record in the data-access layer.
/// </summary>
/// <param name="Id">Unique running identifier for the order. Required. Positive integer.</param>
/// <param name="OrderType">Type/category of the order (e.g., Food, Grocery, Other).</param>
/// <param name="Description">Optional description or notes about the order.</param>
/// <param name="Address">Delivery address for the order.</param>
/// <param name="Latitude">Latitude coordinate of the delivery location (decimal degrees).</param>
/// <param name="Longitude">Longitude coordinate of the delivery location (decimal degrees).</param>
/// <param name="CustomerName">Name of the customer who placed the order.</param>
/// <param name="CustomerPhone">Customer’s contact phone number (E.164 recommended).</param>
/// <param name="Weight">Optional total weight of the order in kilograms (kg).</param>
/// <param name="Volume">Optional total volume of the order in cubic meters (m³).</param>
/// <param name="Fragile">Optional flag indicating whether the order contains fragile items.</param>
/// <param name="OrderTime">Timestamp when the order was placed. Prefer UTC externally.</param>
/// <remarks>
/// Validation guidance (conventions, not enforced by this POCO):
/// - <see cref="Id"/> should be > 0.
/// - <see cref="Latitude"/> must be in range [-90, 90], <see cref="Longitude"/> in [-180, 180].
/// - <see cref="Weight"/> measured in kilograms (kg); <see cref="Volume"/> in cubic meters (m³).
/// - <see cref="CustomerPhone"/> should be a valid phone number (prefer E.164).
/// - If <see cref="OrderTime"/> is set, prefer storing/transmitting as UTC (<see cref="DateTime.UtcNow"/>).
/// </remarks>
/// <example>
/// var order = new Order
/// {
///     Id = 101,
///     OrderType = OrderType.Food,
///     Description = "Vegetarian, no nuts",
///     Address = "123 Main St",
///     Latitude = 32.0853,
///     Longitude = 34.7818,
///     CustomerName = "Dana",
///     CustomerPhone = "+972501234567",
///     Weight = 1.2, // kg
///     Volume = 0.01, // m³
///     Fragile = false,
///     OrderTime = DateTime.UtcNow
/// };
/// </example>
/// <seealso cref="OrderType"/>
public record Order
{
    /// <summary>
    /// Unique running identifier for the order. Required. Should be a positive integer.
    /// </summary>
    public required int Id { get; init; } /// running ID number

    /// <summary>
    /// Category/type of the order. Defaults to <see cref="OrderType.Pizza"/>.
    /// </summary>
    public OrderType OrderType { get; init; } = OrderType.Pizza;

    /// <summary>
    /// Optional description or notes about the order.
    /// Nullable when no description is provided.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// Delivery address for the order. Defaults to "Unknown" when not provided.
    /// </summary>
    public string Address { get; set; } = "Unknown";

    /// <summary>
    /// Latitude coordinate of the delivery location in decimal degrees.
    /// Range: [-90, 90]. Defaults to 0.0.
    /// </summary>
    public double Latitude { get; set; } = 0.0;

    /// <summary>
    /// Longitude coordinate of the delivery location in decimal degrees.
    /// Range: [-180, 180]. Defaults to 0.0.
    /// </summary>
    public double Longitude { get; set; } = 0.0;

    /// <summary>
    /// Customer's full name used for delivery and contact.
    /// Defaults to "Unknown" when not provided.
    /// </summary>
    public string CustomerName { get; set; } = "Unknown";

    /// <summary>
    /// Customer's contact phone number. Prefer E.164 format (e.g., +15551234567).
    /// Defaults to a placeholder when not provided.
    /// </summary>
    public string CustomerPhone { get; set; } = "0000000000";

    /// <summary>
    /// Optional total weight of the order in kilograms (kg). Null when not specified.
    /// </summary>
    public double? Weight { get; set; } = null;

    /// <summary>
    /// Optional total volume of the order in cubic meters (m³). Null when not specified.
    /// </summary>
    public double? Volume { get; set; } = null;

    /// <summary>
    /// Optional flag indicating whether the order contains fragile items.
    /// Null when unspecified.
    /// </summary>
    public bool? Fragile { get; set; } = null;

    /// <summary>
    /// Time when the order was placed. Default is current system time; prefer UTC externally.
    /// </summary>
    public DateTime OrderTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Default parameterless constructor. Use object initializers to populate required properties.
    /// </summary>
    public Order() { }
}
