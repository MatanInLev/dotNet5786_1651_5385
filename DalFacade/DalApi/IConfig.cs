// Module IConfig.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Document the DalApi configuration interface:
/// - Provide XML documentation for each property and the Reset method.
/// - Describe units (km, km/h, TimeSpan semantics), read-only vs read-write properties, and intended usage.
/// - Keep comments in English and consistent with the rest of the DalApi documentation."
/// </aiPrompt>
using System;

namespace DalApi;

/// <summary>
/// Interface exposing DAL configuration values and behaviour used by higher layers (BL, test harness).
/// </summary>
/// <remarks>
/// Implementations typically proxy to a shared configuration store (for example an in-memory singleton).
/// Values include the system clock, admin identifiers, average vehicle speeds (km/h), distance/range limits (km),
/// and timing windows expressed as <see cref="TimeSpan"/> values.
/// </remarks>
public interface IConfig
{
    /// <summary>
    /// System clock used for deterministic tests and time-based calculations.
    /// </summary>
    /// <remarks>
    /// Consumers should prefer storing/transmitting UTC externally; the clock may be set by test harnesses.
    /// </remarks>
    DateTime Clock { get; set; }

    /// <summary>
    /// Maximum allowed range (in kilometers) used by some business rules.
    /// </summary>
    int MaxRange { get; set; }

    /// <summary>
    /// Administrator password used by test harness or admin flows.
    /// </summary>
    string AdminPassword { get; set; }

    /// <summary>
    /// Administrator's ID (T.Z.) used for privileged operations.
    /// </summary>
    int AdminId { get; set; }

    /// <summary>
    /// Company address (optional).
    /// </summary>
    string? CompanyAddress { get; set; }

    /// <summary>
    /// Company latitude coordinate (decimal degrees). Read-only.
    /// </summary>
    double? Latitude { get; }

    /// <summary>
    /// Company longitude coordinate (decimal degrees). Read-only.
    /// </summary>
    double? Longitude { get; }

    /// <summary>
    /// Maximum allowed general distance for deliveries (km). Nullable when not enforced.
    /// </summary>
    double? MaxGeneralDistance { get; set; }

    /// <summary>
    /// Average car speed for ETA estimation (km/h).
    /// </summary>
    double AvgCarSpeed { get; set; }

    /// <summary>
    /// Average motorcycle speed for ETA estimation (km/h).
    /// </summary>
    double AvgMotorcycleSpeed { get; set; }

    /// <summary>
    /// Average bicycle speed for ETA estimation (km/h).
    /// </summary>
    double AvgBicycleSpeed { get; set; }

    /// <summary>
    /// Average walking speed for ETA estimation (km/h).
    /// </summary>
    double AvgFootSpeed { get; set; }

    /// <summary>
    /// Maximum delivery time company commits to (TimeSpan).
    /// </summary>
    TimeSpan MaxDeliveryTime { get; set; }

    /// <summary>
    /// Time span before the max delivery time when an order should be considered 'at risk'.
    /// </summary>
    TimeSpan RiskRange { get; set; }

    /// <summary>
    /// Time span after which an inactive courier is considered unavailable.
    /// </summary>
    TimeSpan InactivityRange { get; set; }

    /// <summary>
    /// Resets configuration values and any counters to their default state.
    /// </summary>
    void Reset();
}
