using System;

namespace BO;

/// <summary>
/// Represents a closed delivery entry shown in order lists (summary of a finished delivery).
/// </summary>
public class ClosedDeliveryInList
{
    /// <summary>
    /// Delivery unique identifier.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// Associated order identifier.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// The order type for the closed delivery.
    /// </summary>
    public OrderType OrderType { get; init; }

    /// <summary>
    /// Customer address for the order.
    /// </summary>
    public string CustomerAddress { get; init; } = null!;

    /// <summary>
    /// Vehicle used for the delivery.
    /// </summary>
    public Vehicle Vehicle { get; init; }

    /// <summary>
    /// Actual distance traveled for this delivery (km), if available.
    /// </summary>
    public double? ActualDistance { get; init; }

    /// <summary>
    /// Time spent processing the delivery (EndTime - StartTime).
    /// </summary>
    public TimeSpan ProcessingTime { get; init; }

    /// <summary>
    /// Final delivery status (Delivered / Refused / Canceled), if known.
    /// </summary>
    public DeliveryStatus? DeliveryEndStatus { get; init; }

    /// <summary>
    /// Returns a textual representation of the object for debugging/logging.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}
