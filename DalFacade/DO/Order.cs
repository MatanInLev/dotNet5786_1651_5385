namespace DO;

public record Order
{
    public required int Id { get; init; }
    public string? Description { get; set; } = null;
    public string Address { get; set; } = "Unknown";
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public string CustomerName { get; set; } = "Unknown";
    public string CustomerPhone { get; set; } = "0000000000";
    public double? weight { get; set; } = null;
    public double? Volume { get; set; } = null;
    public bool Fragile { get; set; } = false;
    public DateTime OrderTime { get; set; } = DateTime.Now;

    public Order() { }
}
