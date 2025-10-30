namespace DalList;

static internal class Config
{
    internal const int OrderStartId = 1;
    private static int _nextOrderId = OrderStartId;
    internal static int NextOrderId { get => ++_nextOrderId; }

    internal const int DeliveryStartId = 1;
    private static int _nextDeliveryId = DeliveryStartId;
    internal static int NextDeliveryId { get => ++_nextDeliveryId; } 

 	internal static DateTime Clock { get; set; } = DateTime.Now;

    internal static void Reset()
    {
        _nextOrderId = OrderStartId;
        _nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;

    }
}
