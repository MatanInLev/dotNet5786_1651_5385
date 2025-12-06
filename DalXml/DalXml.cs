using DalApi;
using System.Diagnostics;
namespace Dal;

internal sealed class DalXml : IDal
{
    public static IDal Instance { get; } = new DalXml();
    private DalXml() { }
    public ICourier Courier { get; } =new CourierImplementation();
    public IOrder Order { get; } = new OrdersImplementation();
    public IDelivery Delivery { get; } = new DeliveryImplementation();
    public IConfig Config { get; } =new ConfigImplementation();
    public void ResetDB()
    {
        Order.DeleteAll();
        Delivery.DeleteAll();
        Courier.DeleteAll();
        Config.Reset();
    }
}
