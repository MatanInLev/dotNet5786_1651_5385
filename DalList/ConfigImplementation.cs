/// <summary>
/// Implements the IConfig interface.
/// This class acts as a proxy, mapping the interface's instance properties
/// to the corresponding static properties in the internal 'Config' class.
/// </summary>
/// 

/*
AI PROMPT (for documentation):
"Create the 'ConfigImplementation.cs' class in the 'DalList' namespace.
- This class must implement the 'IConfig' interface.
- It must act as a proxy, mapping each of its instance properties to the 
  corresponding 'static' property in the 'Config' class 
  (e.t., public DateTime Clock { get => Config.Clock; set => Config.Clock = value; }).
- The 'Reset' method should simply call 'Config.Reset()'."
*/
using DalList;

namespace Dal;

public class ConfigImplementation
{
    
    public DateTime Clock
    {
        get => Config.Instance.Clock;
        set => Config.Instance.Clock = value;
    }
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }
    public string AdminPassword
    {
        get => Config.AdminPassword;
        set => Config.AdminPassword = value;
    }
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }
    public double? Latitude { get => Config.Latitude; }
    public double? Longitude { get => Config.Longitude; }
    public double? MaxGeneralDistance
    {
        get => Config.MaxGeneralDistance;
        set => Config.MaxGeneralDistance = value;
    }
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    public double AvgFootSpeed
    {
        get => Config.AvgFootSpeed;
        set => Config.AvgFootSpeed = value;
    }
    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    public TimeSpan InactivityRange
    {
        get => Config.InactivityRange;
        set => Config.InactivityRange = value;
    }
    public int MaxRange
    {
        get => Config.Instance.MaxRange;
        set => Config.Instance.MaxRange = value;
    }
    public void Reset()
    {
        Config.Instance.Reset();
    }


}
