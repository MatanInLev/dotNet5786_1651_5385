
using DO;

namespace BO;
public class Courier
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public double? MaxDistance { get; set; }
    public Vehicle Vehicle { get; set; }
    public DateTime StartWorkDate { get; init; }
    public int OrdersProvidedOnTime { get; init; }
    public int OrdersProvidedLate { get; init; }
    public OrderInProgress? OrderInProgress { get; set; }

    public override string ToString() => this.ToStringProperty();
}
