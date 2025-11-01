using Dal;
using DalApi;

namespace DalTest
{
    internal class Program
    {   
        /// <summary>
        /// Represents a static instance of the order data access layer.
        /// </summary>
        /// <remarks>This instance is used to interact with the order data storage. It is initialized with
        /// a default implementation of <see cref="OrderImplementation"/>. The instance may be null if not properly
        /// initialized.</remarks>
        private static IOrder? s_dalOrder = new OrderImplementation();
        private static IDelivery? s_dalDelivery = new DeliveryImplementation();
        private static ICourier? s_dalCourier = new CourierImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();

        private static void Main(string[] args)
        {
            // Main method implementation here
        }
    }
}

