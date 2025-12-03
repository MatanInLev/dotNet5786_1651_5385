using System;

namespace BO;

/// <summary>
/// Lightweight courier representation used in lists.
/// </summary>
public class CourierInList
{
    /// <summary>
    /// Courier unique identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Courier full name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Indicates whether the courier is active and available.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Vehicle type used by the courier.
    /// </summary>
    public Vehicle Vehicle { get; init; }

    /// <summary>
    /// Number of orders provided on time.
    /// </summary>
    public int OrdersProvidedOnTime { get; init; }

    /// <summary>
    /// Number of orders provided late.
    /// </summary>
    public int OrdersProvidedLate { get; init; }

    /// <summary>
    /// Current order id the courier is working on, if any.
    /// </summary>
    public int? CurrentOrderId { get; init; }

    /// <summary>
    /// Returns a textual representation of the object for debugging/logging.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}
