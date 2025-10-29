namespace DO;

public record Courier
{
    public required int Id { get; init; }
    public string Name { get; set; } = "Unknown";
    public string PhoneNumber { get; set; } = "Unknown";
    public string Email { get; set; } = "@gmail.com";
    public string Password { get; set; } = "12345678";
    public double? Distance { get; set; } = null;
    public bool IsActive { get; set; } = true;
    public DateTime Date => DateTime.Now;
    public VehicleType VehicleType { get; set; } = VehicleType.Bike;

    public Courier() { }
}