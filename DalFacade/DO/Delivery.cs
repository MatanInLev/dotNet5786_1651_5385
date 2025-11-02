// Module Delivery.cs
namespace DO;

/// <summary>
/// Represents a delivery record in the data-access layer.
/// </summary>
/// <param name="Id">Unique running identifier for the delivery. Required. Positive integer.</param>
/// <param name="OrderId">Identifier of the order being delivered. Required.</param>
/// <param name="CourierId">Identifier of the courier assigned to the delivery. Required.</param>
/// <param name="VehicleType">Type of vehicle used for the delivery (Bike, Car, Foot, Motorcycle).</param>
/// <param name="StartTime">Timestamp when the delivery started. Unit: local system time (prefer UTC).</param>
/// <param name="Distance">Optional travel distance in kilometers. Unit: km.</param>
/// <param name="EndOfDelivery">Optional final outcome of the delivery (Delivered, Canceled, etc.).</param>
/// <param name="EndTime">Optional timestamp when the delivery finished or was aborted. Should be >= <see cref="StartTime"/> when present.</param>
/// <remarks>
/// Validation guidance (conventions, not enforced by this POCO):
/// - <see cref="Id"/>, <see cref="OrderId"/> and <see cref="CourierId"/> should be > 0.
/// - If <see cref="EndTime"/> is set it should be >= <see cref="StartTime"/>.
/// - <see cref="Distance"/> is measured in kilometers and may be null if not recorded.
/// Prefer storing timestamps in UTC (use <see cref="DateTime.UtcNow"/> externally) and convert to local time only for UI.
/// 
/// <aiPrompt>
/// Make the XML documentation explicit, include usage examples, document units/semantics and common validation rules.
/// </aiPrompt>
/// </remarks>
/// <example>
/// var delivery = new Delivery
/// {
///     Id = 1,
///     OrderId = 42,
///     CourierId = 7,
///     VehicleType = VehicleType.Car,
///     StartTime = DateTime.UtcNow,
///     Distance = 12.5,
///     EndOfDelivery = EndOfDelivery.Delivered,
///     EndTime = DateTime.UtcNow.AddMinutes(30)
/// };
/// </example>
/// <seealso cref="VehicleType"/>
/// <seealso cref="EndOfDelivery"/>
public record Delivery
{
    /// <summary>
    /// Unique running identifier for the delivery. Required. Should be a positive integer.
    /// </summary>
    public required int Id { get; init; } /// running ID number

    /// <summary>
    /// Identifier of the associated order. Required.
    /// </summary>
    public required int OrderId { get; init; }

    /// <summary>
    /// Identifier of the courier assigned to this delivery. Required.
    /// </summary>
    public required int CourierId { get; init; }

    /// <summary>
    /// Mode of transportation used for the delivery.
    /// Defaults to <see cref="VehicleType.Bike"/>.
    /// </summary>
    public VehicleType VehicleType { get; set; } = VehicleType.Bike;

    /// <summary>
    /// Time when the delivery was started.
    /// Default is the current system time. Prefer storing and transmitting UTC timestamps.
    /// </summary>
    /// <value>System time indicating when delivery started.</value>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Optional total distance of the delivery in kilometers (km).
    /// Nullable when the distance is not recorded.
    /// </summary>
    public double? Distance { get; init; } = null;

    /// <summary>
    /// Optional final state of the delivery (Delivered, Canceled, Failed, Refused, NotThere).
    /// Null while the delivery is in progress.
    /// </summary>
    public EndOfDelivery? EndOfDelivery { get; init; } = null;

    /// <summary>
    /// Optional time when the delivery completed (or failed/canceled).
    /// Should be >= <see cref="StartTime"/> when present.
    /// </summary>
    public DateTime? EndTime { get; init; } = null;

    /// <summary>
    /// Default parameterless constructor. Use object initializers to populate required properties.
    /// </summary>
    public Delivery() { }
}
