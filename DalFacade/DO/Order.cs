
namespace DO;

public record Order
{
    public required int Id { get; init; }
    public string? Description { get; set; }
    public string Address { get; set; }
}
