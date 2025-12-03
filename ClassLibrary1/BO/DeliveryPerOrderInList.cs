using System;

namespace BO;

/// <summary>
/// A delivery entry shown inside an Order details page (per-order delivery summary).
/// </summary>
public class DeliveryPerOrderInList
{
    /// <summary>
    /// Delivery unique identifier.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// Courier unique identifier who executed the delivery.
    /// </summary>
    public int CourierId { get; init; }

    /// <summary>
    /// Courier display name.
    /// </summary>
    public string CourierName { get; init; } = null!;

    /// <summary>
    /// Vehicle used by the courier for this delivery.
    /// </summary>
    public Vehicle Vehicle { get; init; }

    /// <summary>
    /// Delivery start date/time.
    /// </summary>
    public DateTime StartDeliveryDate { get; init; }

    /// <summary>
    /// Delivery end date/time, null if still in progress.
    /// </summary>
    public DateTime? EndDeliveryDate { get; init; }

    /// <summary>
    /// Delivery final status (Delivered / Refused / Canceled), if known.
    /// </summary>
    public DeliveryStatus? Status { get; init; }

    /// <summary>
    /// Returns a textual representation of the object for debugging/logging.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}
