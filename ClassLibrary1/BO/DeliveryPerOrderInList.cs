using System;

namespace BO;

public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int CourierId { get; init; }
    public string CourierName { get; init; } = null!;
    public Vehicle Vehicle { get; init; }
    public DateTime StartDeliveryDate { get; init; }
    public DateTime? EndDeliveryDate { get; init; }
    public DeliveryStatus? Status { get; init; }

    public override string ToString() => this.ToStringProperty();
}
