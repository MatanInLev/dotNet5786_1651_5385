namespace DalList;

static internal class Config
{
    internal const int OrderStartId = 1;
    private static int nextOrderId = OrderStartId;
    internal static int NextOrderId { get => ++nextOrderId; }

    internal const int DeliveryStartId = 1;
    private static int nextDeliveryId = DeliveryStartId;
    internal static int NextDeliveryId { get => ++nextDeliveryId; } 

 	internal static DateTime Clock { get; set; } = DateTime.Now;

    internal static void Reset()
    {
        nextOrderId = OrderStartId;
        nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;

    }
}
