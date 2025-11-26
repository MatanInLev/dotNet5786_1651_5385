using System;

namespace BO;

public class OrderInList
{
    public int Id { get; init; }
    public int? DeliveryId { get; init; }
    public OrderType Type { get; init; }
    public double Distance { get; init; }
    public OrderStatus Status { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeft { get; init; }
    public TimeSpan TotalProcessingTime { get; init; }
    public int AmountOfDeliveries { get; init; }

    public override string ToString() => this.ToStringProperty();
}
