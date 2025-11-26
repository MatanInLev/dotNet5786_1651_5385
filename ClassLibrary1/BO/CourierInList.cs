using System;

namespace BO;

public class CourierInList
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsActive { get; init; }
    public Vehicle Vehicle { get; init; }
    public int OrdersProvidedOnTime { get; init; }
    public int OrdersProvidedLate { get; init; }
    public int? CurrentOrderId { get; init; }

    public override string ToString() => this.ToStringProperty();
}
