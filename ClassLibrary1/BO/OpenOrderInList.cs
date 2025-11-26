namespace BO;

public class OpenOrderInList
{
    public int Id { get; init; }
    public int? CourierId { get; init; }
    public OrderType Type { get; init; }
    public string CustomerAddress { get; init; } = null!;
    public double DistanceFromCompany { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan? EstimatedDeliveryTime { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeLeft { get; init; }
    public DateTime MaxDeliveryDate { get; init; }

    public override string ToString() => this.ToStringProperty();
}
