///Module Courier.cs

/*
AI PROMPT (for documentation):
"Based on the project specification PDFs, generate the 'DO.Courier' entity as a 
C# record.
- Ensure all properties match the 'ישות נתונים שליח' table.
- Use nullable types where specified (e.g., MaxDistance).
- Omit the 'Password' property as it is an optional add-on.
- Include a parameterless constructor for XML serialization.
- All documentation comments (///) must be in English."
*/
namespace DO;

/// <summary>
/// Courier Entity
/// </summary>
/// <param name="Id">Unique ID of the courier (required field)</param>
/// <param name="Name">Courier’s full name</param>
/// <param name="PhoneNumber">Courier’s contact phone number</param>
/// <param name="Email">Courier’s email address</param>
/// <param name="Password">Courier’s login password</param>
/// <param name="Distance">Current delivery distance or range (in kilometers)</param>
/// <param name="IsActive">Indicates whether the courier is active or not</param>
/// <param name="Date">Date and time when the data was accessed (auto-generated)</param>
/// <param name="VehicleType">Courier’s vehicle type (e.g. bike, car, scooter)</param>
public record Courier
{
    public required int Id { get; init; }
    public string Name { get; set; } = "Unknown";
    public string PhoneNumber { get; set; } = "Unknown";
    public string Email { get; set; } = "@gmail.com";
    public double? Distance { get; set; } = null;
    public bool IsActive { get; set; } = true;
    public DateTime Date => DateTime.Now;
    public VehicleType VehicleType { get; set; } = VehicleType.Bike;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Courier() {  }


}
