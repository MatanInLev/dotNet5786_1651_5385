using System;

namespace BO;

/// <summary>
/// Lightweight summary representation of an order for list views.
/// </summary>
public class OrderInList
{
    /// <summary>Order unique identifier.</summary>
    public int Id { get; init; }

    /// <summary>Active delivery id if the order is currently assigned; otherwise null.</summary>
    public int? DeliveryId { get; init; }

    /// <summary>Order type/category.</summary>
    public OrderType Type { get; init; }

    /// <summary>Distance from company to customer (kilometers).</summary>
    public double Distance { get; init; }

    /// <summary>High-level order status.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Schedule evaluation status (OnTime / Risk / Late).</summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>Time left until the delivery deadline.</summary>
    public TimeSpan TimeLeft { get; init; }

    /// <summary>Total processing time across all deliveries for this order.</summary>
    public TimeSpan TotalProcessingTime { get; init; }

    /// <summary>Number of deliveries performed for this order.</summary>
    public int AmountOfDeliveries { get; init; }

    /// <summary>Returns a textual representation of the object for debugging/logging.</summary>
    public override string ToString() => this.ToStringProperty();
}
