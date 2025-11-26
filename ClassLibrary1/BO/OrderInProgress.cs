using System;

namespace BO;

public class OrderInProgress
{
    public int DeliveryId { get; init; }

    public int OrderId { get; init; }

    public OrderType Type { get; init; }

    public string? Description { get; init; } 

    public string CustomerAddress { get; init; } = null!;

    public double Distance { get; init; }

    public double? ActualDistance { get; init; } 

    public string CustomerName { get; init; } = null!;

    public string CustomerPhone { get; init; } = null!;

    public DateTime OrderDate { get; init; }

    public DateTime StartDeliveryDate { get; init; }

    public DateTime ExpectedDelivery { get; init; } 

    public DateTime MaxDelivery { get; init; }

    public OrderStatus Status { get; init; }

    public ScheduleStatus ScheduleStatus { get; init; }

    public TimeSpan TimeLeft { get; init; } 

    public override string ToString() => this.ToStringProperty();
}