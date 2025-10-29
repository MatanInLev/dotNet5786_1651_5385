namespace DO;

public record Courier
{
    public required int Id { get; init; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } = "12345678";
    public double? Distance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime Date => DateTime.Now;
    public DeliveryType DeliveryType { get; set; }
}