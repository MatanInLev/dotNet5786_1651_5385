namespace BO;

/// <summary>
/// System configuration settings used by the business layer.
/// </summary>
public class Config
{
    /// <summary>
    /// Maximum allowed delivery range (km).
    /// </summary>
    public int MaxRange { get; set; }

    /// <summary>
    /// Company address (optional).
    /// </summary>
    public string? CompanyAddress { get; set; }

    /// <summary>
    /// Average car speed used for routing/time estimates (km/h).
    /// </summary>
    public double CarSpeed { get; set; }

    /// <summary>
    /// Average motorcycle speed used for routing/time estimates (km/h).
    /// </summary>
    public double MotorcycleSpeed { get; set; }

    /// <summary>
    /// Average bicycle speed used for routing/time estimates (km/h).
    /// </summary>
    public double BicycleSpeed { get; set; }

    /// <summary>
    /// Average foot speed used for routing/time estimates (km/h).
    /// </summary>
    public double FootSpeed { get; set; }

    /// <summary>
    /// Maximum allowed delivery time.
    /// </summary>
    public TimeSpan MaxDeliveryTime { get; set; }

    /// <summary>
    /// Time range before deadline considered as "risk".
    /// </summary>
    public TimeSpan RiskRange { get; set; }

    /// <summary>
    /// Time of inactivity threshold used by the system.
    /// </summary>
    public TimeSpan InactivityTime { get; set; }
}
