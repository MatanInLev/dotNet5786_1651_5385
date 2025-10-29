namespace DO;

internal class Delivery
{
    public required int Id { get; init; }
    public required int OrderId { get; init; }
    public required int CourierId { get; init; }
    public OrderType OrderType { get; init; } = OrderType.Other;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public double? Distance { get; init; } = null;
    public EndOfDelivery? EndOfDelivery { get; init; } = null;
    public DateTime? EndTime { get; init; } = null;

    public Delivery() { }
}
