using DalApi;
using System;

namespace DalList;

/**
 * <summary>
 * The Configuration entity (Config) for the Data Access Layer (DAL).
 * Implements the Singleton pattern to hold a single instance of all 
 * global configuration settings (Clock, Speeds, Admin details, etc.) and
 * manages auto-incrementing ID counters.
 * The class also implements the IConfig interface for external access to configurable members.
 * </summary>
 */
internal class Config
{
    // Singleton Instance to allow global access to the single configuration object
    internal static Config Instance { get; } = new();

    // Private constructor to enforce the Singleton pattern
    private Config() { }

    // --- Auto-Incrementing ID Logic (Instance Members) ---

    internal const int OrderStartId = 1;
    private int _nextOrderId = OrderStartId;
    /// <summary>
    /// Represents the ID for the next new Order entity. 
    /// The property automatically increments by 1 when retrieved.
    /// </summary>
    internal int NextOrderId { get => _nextOrderId++; }

    internal const int DeliveryStartId = 1;
    private int _nextDeliveryId = DeliveryStartId;
    /// <summary>
    /// Represents the ID for the next new Delivery entity.
    /// The property automatically increments by 1 when retrieved.
    /// </summary>
    internal int NextDeliveryId { get => _nextDeliveryId++; }

    // --- Configuration Properties (Mixed Static/Instance Members) ---

    /// <summary>
    /// The system clock used for time simulation in the application.
    /// Can be updated by the manager or simulator.
    /// </summary>
    internal DateTime Clock { get; set; } = DateTime.Now; // Instance member

    // Static Members
    /// <summary>The official ID (T.Z.) of the system administrator.</summary>
    internal static int AdminId { get; set; } = 123456782;
    /// <summary>The password for the system administrator (Optional Add-on).</summary>
    internal static string AdminPassword { get; set; } = "admin123";
    /// <summary>The full, real address of the company base. Can be null if invalid.</summary>
    internal static string? CompanyAddress { get; set; } = null;
    /// <summary>The calculated latitude of the company address. Can be null if invalid address.</summary>
    internal static double? Latitude { get; set; } = null;
    /// <summary>The calculated longitude of the company address. Can be null if invalid address.</summary>
    internal static double? Longitude { get; set; } = null;
    /// <summary>The maximum allowed aerial distance (in km) for a delivery from the company base. Null means no restriction.</summary>
    internal static double? MaxGeneralDistance { get; set; } = null;
    /// <summary>The average speed for car deliveries (km/h).</summary>
    internal static double AvgCarSpeed { get; set; } = 60;
    /// <summary>The average speed for motorcycle deliveries (km/h).</summary>
    internal static double AvgMotorcycleSpeed { get; set; } = 70;
    /// <summary>The average speed for bicycle deliveries (km/h).</summary>
    internal static double AvgBicycleSpeed { get; set; } = 20;
    /// <summary>The average speed for on-foot deliveries (km/h).</summary>
    internal static double AvgFootSpeed { get; set; } = 5;
    /// <summary>The maximum delivery time the company commits to (e.g., 2 hours).</summary>
    internal static TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromHours(2);
    /// <summary>The time range before the maximum delivery time where an order is considered 'In Risk' (e.g., 15 minutes).</summary>
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(15);
    /// <summary>The time span of inactivity after which a courier is marked as 'Inactive' (e.g., 3 days).</summary>
    internal static TimeSpan InactivityRange { get; set; } = TimeSpan.FromDays(3);

    /// <summary>The maximum range value (e.g., 50 km) available in the configuration settings. (Implemented from IConfig)</summary>
    public int MaxRange { get; set; } = 50; // Instance member

    /// --- Reset Method (IConfig Instance Member) ---

    /// <summary>
    /// Resets all ID counters and configuration properties to their initial default values.
    /// </summary>
    public void Reset()
    {
        // Reset Instance members
        _nextOrderId = OrderStartId;
        _nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;
        MaxRange = 50;

        // Reset Static members (must be accessed via the class name 'Config')
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