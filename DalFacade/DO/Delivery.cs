// Module Delivery.cs
namespace DO;

/// <summary>
/// Delivery Entity
/// </summary>
/// <param name="Id">Unique ID of the delivery (required field)</param>
/// <param name="OrderId">Associated order ID linked to this delivery</param>
/// <param name="CourierId">ID of the courier assigned to the delivery</param>
/// <param name="OrderType">of the order being delivered</param>
/// <param name="StartTime">Time when the delivery started</param>
/// <param name="Distance">Delivery distance (in kilometers)</param>
/// <param name="EndOfDelivery">Delivery completion status or result</param>
/// <param name="EndTime">Time when the delivery ended (if completed)</param>
public record Delivery
{
    public required int Id { get; init; }/// running ID number
    public required int OrderId { get; init; }
    public required int CourierId { get; init; }
    public VehicleType VehicleType { get; set; } = VehicleType.Bike;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public double? Distance { get; init; } = null;
    public EndOfDelivery? EndOfDelivery { get; init; } = null;
    public DateTime? EndTime { get; init; } = null;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Delivery() { }
}
