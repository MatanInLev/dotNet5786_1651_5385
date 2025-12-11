using System;
using DO;

namespace BO;
/// <summary>
/// Represents a courier in the business layer.
/// </summary>
public class Courier
{
    /// <summary>
    /// Unique identifier of the courier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Full name of the courier.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Contact phone number of the courier.
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// Contact email address of the courier.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Indicates whether the courier is currently active and available for assignments.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Maximum delivery distance the courier can travel in kilometers; null if unlimited or unspecified.
    /// </summary>
    public double? MaxDistance { get; set; }

    /// <summary>
    /// The vehicle used by the courier.
    /// </summary>
    public Vehicle Vehicle { get; set; }

    /// <summary>
    /// The date the courier started working.
    /// Defaults to today for newly constructed instances to avoid DateTime.MinValue.
    /// </summary>
    public DateTime StartWorkDate { get; init; } = DateTime.Today;

    /// <summary>
    /// Count of orders the courier delivered on time.
    /// </summary>
    public int OrdersProvidedOnTime { get; init; }

    /// <summary>
    /// Count of orders the courier delivered late.
    /// </summary>
    public int OrdersProvidedLate { get; init; }

    /// <summary>
    /// The order currently in progress by the courier, if any.
    /// </summary>
    public OrderInProgress? OrderInProgress { get; set; }

    /// <summary>
    /// Returns a descriptive string representation of the courier.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}
