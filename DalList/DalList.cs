using DalApi;

namespace Dal;

public sealed class DalList : IDal

{
    public ICourier Courier { get; }= new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IDelivery Delivery { get; } = new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll(); 
        Delivery.DeleteAll();
        Config.Reset();
    }
}
