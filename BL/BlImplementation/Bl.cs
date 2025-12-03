namespace BlImplementation;
using BlApi;
internal class Bl : IBl
{
    public IOrder Order { get; } = new OrderImplementation();

    public ICourier Courier { get; } = new CourierImplementation();

    public IAdmin Admin { get; } = new AdminImplementation();
}
