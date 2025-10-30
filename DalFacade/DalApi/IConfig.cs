namespace DalApi;

/*
AI PROMPT (for documentation):
"Based on the project specifications (Stage 1, page 18) and the full list of
properties in the static 'Config' class, create the public 'IConfig.cs' interface.
- This interface should be placed in the 'DalFacade/DalApi' namespace.
- It must expose all configuration properties that the Business Logic layer 
  needs to access (e.t., Clock, AdminId, AvgCarSpeed, etc.).
- The 'Latitude' and 'Longitude' properties should be read-only (get-only)."
*/

public interface IConfig
{
    DateTime Clock { get; set; }
    int MaxRange { get; set; }
    string AdminPassword { get; set; }
    int AdminId { get; set; }
    string? CompanyAddress { get; set; }
    double? Latitude { get; }
    double? Longitude { get; }
    double? MaxGeneralDistance { get; set; }
    double AvgCarSpeed { get; set; }
    double AvgMotorcycleSpeed { get; set; }
    double AvgBicycleSpeed { get; set; }
    double AvgFootSpeed { get; set; }
    TimeSpan MaxDeliveryTime { get; set; }
    TimeSpan RiskRange { get; set; }
    TimeSpan InactivityRange { get; set; }
    void Reset();
}
