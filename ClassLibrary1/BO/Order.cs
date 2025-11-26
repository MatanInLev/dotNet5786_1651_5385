
using System;

namespace BO;

public class Order
{
    public int Id { get; init; }
    public OrderType Type { get; set; }
    public string? Description { get; set; }
    public string CustomerAddress { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Distance { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public DateTime OrderDate { get; init; }
    public DateTime ExpectedDelivery { get; init; }
    public DateTime MaxDelivery { get; init; }
    public OrderStatus Status { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeft { get; init; }
    public List<DeliveryPerOrderInList>? DeliveryList { get; set; }

    public override string ToString() => this.ToStringProperty();
}
