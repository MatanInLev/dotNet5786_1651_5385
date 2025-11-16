/// <summary>
/// Implements the IConfig interface.
/// This class acts as a proxy, mapping the interface's instance properties
/// to the corresponding static properties in the internal 'Config' class.
/// </summary>
/// 

/// <summary>
/// AI PROMPT (for documentation):
/// "Create the 'ConfigImplementation.cs' class in the 'DalList' namespace.
/// - This class must implement the 'IConfig' interface.
/// - It must act as a proxy, mapping each of its instance properties to the 
///   corresponding 'static' property in the 'Config' class 
///   (e.t., public DateTime Clock { get => Config.Clock; set => Config.Clock = value; }).
/// - The 'Reset' method should simply call 'Config.Reset()'."
/// </summary>

using DalList;
using DalApi;
using System;

namespace Dal;

/// <summary>
/// Implementation of the <see cref="IConfig"/> interface that proxies access to the internal <see cref="Config"/> singleton.
/// </summary>
/// <remarks>
/// This class maps each instance property required by <see cref="IConfig"/> to the corresponding
/// <see cref="Config"/> instance or static member. The purpose is to present an interface-bound object to callers
/// (e.g., test harness or BL) while keeping the actual configuration storage centralized in <see cref="Config.Instance"/>.
/// Behavior notes:
/// - Instance-scoped properties (like <see cref="IConfig.Clock"/> and <see cref="IConfig.MaxRange"/>) are forwarded
///   to <see cref="Config.Instance"/> so that Reset semantics and instance-state are preserved.
/// - Static defaults in <see cref="Config"/> are forwarded by getters/setters where appropriate.
/// - <see cref="Reset"/> invokes <see cref="Config.Reset"/> which resets both instance counters and static defaults.
/// - This proxy does not add validation; callers must ensure values are meaningful before assignment.
/// </remarks>
internal class ConfigImplementation : IConfig
{
    /// <summary>
    /// Gets or sets the system clock exposed via the <see cref="IConfig"/> interface.
    /// </summary>
    /// <remarks>
    /// The value is stored in <see cref="Config.Instance.Clock"/>. Setting the clock affects tests and
    /// any code that reads the Clock via the DAL-proxy or directly from Config.Instance.
    /// </remarks>
    public DateTime Clock
    {
        get => Config.Instance.Clock;
        set => Config.Instance.Clock = value;
    }

    /// <summary>
    /// Gets or sets the administrator's ID (T.Z.) exposed via <see cref="IConfig"/>. Maps to <see cref="Config.AdminId"/>.
    /// </summary>
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }

    /// <summary>
    /// Gets or sets the company address string. Maps to <see cref="Config.CompanyAddress"/>.
    /// </summary>
    /// <remarks>
    /// The address may be null if not configured. Geographic coordinates are stored separately in <see cref="Latitude"/>
    /// and <see cref="Longitude"/> which are read-only in this proxy.
    /// </remarks>
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }

    /// <summary>
    /// Gets the configured company latitude (read-only). Maps to <see cref="Config.Latitude"/>.
    /// </summary>
    public double? Latitude => Config.Latitude;

    /// <summary>
    /// Gets the configured company longitude (read-only). Maps to <see cref="Config.Longitude"/>.
    /// </summary>
    public double? Longitude => Config.Longitude;

    /// <summary>
    /// Gets or sets the maximum general distance allowed for deliveries (km). Maps to <see cref="Config.MaxGeneralDistance"/>.
    /// </summary>
    public double? MaxGeneralDistance
    {
        get => Config.MaxGeneralDistance;
        set => Config.MaxGeneralDistance = value;
    }

    /// <summary>
    /// Average car speed used for delivery ETA calculations (km/h). Maps to <see cref="Config.AvgCarSpeed"/>.
    /// </summary>
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    /// <summary>
    /// Average motorcycle speed used for delivery ETA calculations (km/h). Maps to <see cref="Config.AvgMotorcycleSpeed"/>.
    /// </summary>
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    /// <summary>
    /// Average bicycle speed used for delivery ETA calculations (km/h). Maps to <see cref="Config.AvgBicycleSpeed"/>.
    /// </summary>
    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    /// <summary>
    /// Average walking speed used for delivery ETA calculations (km/h). Maps to <see cref="Config.AvgFootSpeed"/>.
    /// </summary>
    public double AvgFootSpeed
    {
        get => Config.AvgFootSpeed;
        set => Config.AvgFootSpeed = value;
    }

    /// <summary>
    /// Maximum delivery time company commits to. Maps to <see cref="Config.MaxDeliveryTime"/>.
    /// </summary>
    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    /// <summary>
    /// Risk range before max delivery time. Maps to <see cref="Config.RiskRange"/>.
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    /// <summary>
    /// Time span after which a courier is considered inactive. Maps to <see cref="Config.InactivityRange"/>.
    /// </summary>
    public TimeSpan InactivityRange
    {
        get => Config.InactivityRange;
        set => Config.InactivityRange = value;
    }

    /// <summary>
    /// Maximum allowed range exposed via the instance proxy. Maps to <see cref="Config.Instance.MaxRange"/>.
    /// </summary>
    /// <remarks>
    /// This property is instance-scoped on the singleton and intended for callers that obtain an <see cref="IConfig"/>.
    /// </remarks>
    public int MaxRange
    {
        get => Config.Instance.MaxRange;
        set => Config.Instance.MaxRange = value;
    }

    /// <summary>
    /// Resets the configuration to defaults by delegating to <see cref="Config.Reset"/>.
    /// </summary>
    /// <remarks>
    /// After calling <see cref="Reset"/>, ID counters, the instance clock, and static defaults are restored to their initial values.
    /// Use this method to return the system to a clean initial state in tests or before re-initialization.
    /// </remarks>
    public void Reset() => Config.Instance.Reset();
}
