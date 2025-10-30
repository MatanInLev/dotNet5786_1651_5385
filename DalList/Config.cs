using DalApi;
namespace DalList;

 internal class Config: IConfig
{
    internal static Config Instance { get; } = new();
    internal const int OrderStartId = 1;
    private  int _nextOrderId = OrderStartId;
    internal  int NextOrderId { get => ++_nextOrderId; }

    internal const int DeliveryStartId = 1;
    private  int _nextDeliveryId = DeliveryStartId;
    internal int NextDeliveryId { get => ++_nextDeliveryId; } 

 	public DateTime Clock { get; set; } = DateTime.Now;
    public int MaxRange { get; set; } = 50;

    public void Reset()
    {
        _nextOrderId = OrderStartId;
        _nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;
        MaxRange = 50;
    }
}
