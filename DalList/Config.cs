namespace DalList;

static internal class Config
{
    const internal int OrderStartId = 1;
    private static int nextOrderId = OrderStartId;
    internal static int NextOrderId { get => ++nextOrderId; }

    const internal int DeliveryStartId = 1;
    private static int nextDeliveryId = DeliveryStartId;
    internal static in NextDeliveryId { get => ++nextDeliveryId; } 

 	internal static DateTime Clock { get; set; } = DateTime.Now;

    internal static void Reset()
    {
        nextOrderId = OrderStartId;
        nextDeliveryId = DeliveryStartId;
        Clock = DateTime.Now;

    }
}
