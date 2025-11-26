
using System;

namespace BO;

public class ClosedDeliveryInList
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; init; }
    public string CustomerAddress { get; init; } = null!;
    public Vehicle Vehicle { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public DeliveryStatus? DeliveryEndStatus { get; init; }

    public override string ToString() => this.ToStringProperty();
}
