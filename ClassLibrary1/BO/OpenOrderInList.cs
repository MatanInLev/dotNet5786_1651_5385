namespace BO;

/// <summary>
/// Representation of an open order as shown to couriers (available assignments).
/// </summary>
public class OpenOrderInList
{
    /// <summary>Order unique identifier.</summary>
    public int Id { get; init; }

    /// <summary>Courier id viewing the list (optional).</summary>
    public int? CourierId { get; init; }

    /// <summary>Order type/category.</summary>
    public OrderType Type { get; init; }

    /// <summary>Customer delivery address.</summary>
    public string CustomerAddress { get; init; } = null!;

    /// <summary>Aerial distance from company to customer (kilometers).</summary>
    public double DistanceFromCompany { get; init; }

    /// <summary>Actual route distance if known; otherwise null.</summary>
    public double? ActualDistance { get; init; }

    /// <summary>Estimated delivery time for the courier, if available.</summary>
    public TimeSpan? EstimatedDeliveryTime { get; init; }

    /// <summary>Schedule evaluation status (OnTime / Risk / Late).</summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>Time left until the delivery deadline.</summary>
    public TimeSpan TimeLeft { get; init; }

    /// <summary>Maximum allowed delivery date/time for the order.</summary>
    public DateTime MaxDeliveryDate { get; init; }

    /// <summary>Returns a textual representation of the object for debugging/logging.</summary>
    public override string ToString() => this.ToStringProperty();
}
