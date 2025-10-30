using DalApi;
namespace DalList;

/*
AI PROMPT (for documentation):
"Based on the 'ישות תצורה' table (General PDF) and the 'הגדרת ישות תצורה' 
section (Stage 1 PDF, page 13), create a *static* class 'Config.cs' 
in the 'DalList' namespace.
- It must include all configuration properties (Clock, AdminId, Speeds, etc.).
- It must include the static auto-incrementing ID logic for 'NextOrderId' 
  and 'NextDeliveryId'.
- It must implement a static 'Reset()' method to restore all default values."
*/

internal class Config : IConfig
{
    internal static Config Instance { get; } = new();

    internal const int OrderStartId = 1;
    private  int _nextOrderId = OrderStartId;
    internal  int NextOrderId { get => _nextOrderId++; }

    internal const int DeliveryStartId = 1;
    private  int _nextDeliveryId = DeliveryStartId;
    internal int NextDeliveryId { get => _nextDeliveryId++; } 

 	internal DateTime Clock { get; set; } = DateTime.Now;
    internal static int AdminId { get; set; } = 123456782;
    internal static string AdminPassword { get; set; } = "admin123";
    internal static string? CompanyAddress { get; set; } = null;
    internal static double? Latitude { get; set; } = null;
    internal static double? Longitude { get; set; } = null;
    internal static double? MaxGeneralDistance { get; set; } = null;
    internal static double AvgCarSpeed { get; set; } = 60;
    internal static double AvgMotorcycleSpeed { get; set; } = 70;
    internal static double AvgBicycleSpeed { get; set; } = 20;
    internal static double AvgFootSpeed { get; set; } = 5;
    internal static TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromHours(2);
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(15);
    internal static TimeSpan InactivityRange { get; set; } = TimeSpan.FromDays(3);

    public int MaxRange { get; set; } = 50;

    public void Reset()
    {
        _nextOrderId = OrderStartId;
        _nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;
        MaxRange = 50;
        

        /// Reset Static members (must be accessed via the class name 'Config')
        Config.AdminId = 123456782;
        Config.AdminPassword = "admin123";
        Config.CompanyAddress = null;
        Config.Latitude = null;
        Config.Longitude = null;
        Config.MaxGeneralDistance = null;
        Config.AvgCarSpeed = 60;
        Config.AvgMotorcycleSpeed = 70;
        Config.AvgBicycleSpeed = 20;
        Config.AvgFootSpeed = 5;
        Config.MaxDeliveryTime = TimeSpan.FromHours(2);
        Config.RiskRange = TimeSpan.FromMinutes(15);
        Config.InactivityRange = TimeSpan.FromDays(3);
    }
}
