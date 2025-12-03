using System;

namespace BO;

/// <summary>
/// Detailed representation of an order that is currently in progress (active delivery).
/// </summary>
public class OrderInProgress
{
    /// <summary>Associated delivery identifier.</summary>
    public int DeliveryId { get; init; }

    /// <summary>Associated order identifier.</summary>
    public int OrderId { get; init; }

    /// <summary>Order type/category.</summary>
    public OrderType Type { get; init; }

    /// <summary>Optional order description.</summary>
    public string? Description { get; init; } 

    /// <summary>Customer delivery address.</summary>
    public string CustomerAddress { get; init; } = null!;

    /// <summary>Aerial distance (kilometers) planned for this order.</summary>
    public double Distance { get; init; }

    /// <summary>Actual route distance if known; otherwise null.</summary>
    public double? ActualDistance { get; init; } 

    /// <summary>Customer full name.</summary>
    public string CustomerName { get; init; } = null!;

    /// <summary>Customer phone number.</summary>
    public string CustomerPhone { get; init; } = null!;

    /// <summary>Order creation date/time.</summary>
    public DateTime OrderDate { get; init; }

    /// <summary>Delivery start date/time.</summary>
    public DateTime StartDeliveryDate { get; init; }

    /// <summary>Expected delivery date/time.</summary>
    public DateTime ExpectedDelivery { get; init; } 

    /// <summary>Maximum allowed delivery date/time.</summary>
    public DateTime MaxDelivery { get; init; }

    /// <summary>High-level order status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Schedule evaluation status (OnTime / Risk / Late).</summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>Time left until the delivery deadline.</summary>
    public TimeSpan TimeLeft { get; init; } 

    /// <summary>Returns a textual representation of the object for debugging/logging.</summary>
    public override string ToString() => this.ToStringProperty();
}