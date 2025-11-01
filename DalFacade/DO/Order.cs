/// Module Order.cs
/// 
/*
AI PROMPT (for documentation):
"Based on the project specification PDFs (General + Stage 1), generate the 'DO.Order'
 entity as a C# record. 
- Ensure all properties match the 'ישות נתונים הזמנה' table.
- Use nullable types where specified (e.g., Description, Volume).
- Include a parameterless constructor for XML serialization, as required by Stage 3.
- All documentation comments (///) must be in English."
*/
namespace DO;

/// <summary>
/// Order Entity
/// </summary>
/// <param name="Id">Unique ID of the order (required field)</param>
/// <param name="Description">Optional description or notes about the order</param>
/// <param name="Address">Delivery address for the order</param>
/// <param name="Latitude">Latitude coordinate of the delivery location</param>
/// <param name="Longitude">Longitude coordinate of the delivery location</param>
/// <param name="CustomerName">Name of the customer who placed the order</param>
/// <param name="CustomerPhone">Customer’s contact phone number</param>
/// <param name="Weight">Order’s total weight (in kilograms, if applicable)</param>
/// <param name="Volume">Order’s total volume (in cubic meters, if applicable)</param>
/// <param name="Fragile">Indicates whether the order contains fragile items</param>
/// <param name="OrderTime">Time when the order was placed</param>
public record Order
{
    public required int Id { get; init; }/// running ID number
    public OrderType OrderType { get; init; } = OrderType.Other;
    public string? Description { get; set; } = null;
    public string Address { get; set; } = "Unknown";
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public string CustomerName { get; set; } = "Unknown";
    public string CustomerPhone { get; set; } = "0000000000";
    public double? Weight { get; set; } = null;
    public double? Volume { get; set; } = null;
    public bool? Fragile { get; set; } = null;
    public DateTime OrderTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Order() { }
}
