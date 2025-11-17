using DalApi;
namespace Dal;

public class DalXml : IDal
{
    public ICourier Courier { get; } =new CourierImplementation();
    public IOrder Order { get; } = new OrdersImplementation();
    public IDelivery Delivery { get; } = new DeliveryImplementation();
    public IConfig Config { get; } =new ConfigImplementation();
    public void ResetDB()
    {
        Order.DeleteAll();
        Delivery.DeleteAll();
        Order.DeleteAll();
        Config.Reset();
    }
}
