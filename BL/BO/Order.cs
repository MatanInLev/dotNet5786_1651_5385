using System;

namespace BO;

/// <summary>
/// Represents a full order in the business layer.
/// </summary>
public class Order
{
    /// <summary>
    /// Order unique identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Order type/category.
    /// </summary>
    public OrderType Type { get; set; }

    /// <summary>
    /// Optional order description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Customer delivery address.
    /// </summary>
    public string CustomerAddress { get; set; } = null!;

    /// <summary>
    /// Customer latitude coordinate.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Customer longitude coordinate.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Calculated or stored distance for the order.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Customer full name.
    /// </summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>
    /// Customer phone number.
    /// </summary>
    public string CustomerPhone { get; set; } = null!;

    /// <summary>
    /// Order creation date/time.
    /// </summary>
    public DateTime OrderDate { get; init; }

    /// <summary>
    /// Expected delivery date/time.
    /// </summary>
    public DateTime ExpectedDelivery { get; init; }

    /// <summary>
    /// Latest allowed delivery date/time.
    /// </summary>
    public DateTime MaxDelivery { get; init; }

    /// <summary>
    /// Current order status.
    /// </summary>
    public OrderStatus Status { get; init; }

    /// <summary>
    /// Schedule status (OnTime, Risk, Late).
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// Time left until MaxDelivery or related deadline.
    /// </summary>
    public TimeSpan TimeLeft { get; init; }

    /// <summary>
    /// List of deliveries associated with this order (history).
    /// </summary>
    public List<DeliveryPerOrderInList>? DeliveryList { get; set; }

    /// <summary>
    /// Returns a textual representation of the object for debugging/logging.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}
