using DalApi;
using DO;
namespace DalTest;

public static class Initialization
{
    private static IConfig? s_dalIConfig;
    private static ICourier? s_dalICourier;
    private static IDelivery? s_dalIDelivery;
    private static IOrder? s_dalIOrder;

    ///random parameter
    private static readonly Random s_rand = new();
    /// <summary>
    /// Customer names for randomizer
    /// </summary>
    private static readonly string[] s_customerNames =
    {
        "Israel Israeli", "Moshe Cohen", "Avi Levi", "Sarah Schwartz", "Rivkah Avram",
        "David Biton", "Yosef Kaplan", "Chana Berkovich", "Miriam Hadad", "Shlomo Friedman"
    };
    /// <summary>
    /// Courier names for randomizer
    /// </summary>
    private static readonly string[] s_courierNames =
    {
        "Daniel Levi", "Eliran Golan", "Yair Kahana", "Ariela Levin", "Dina Klein",
        "Shir Israeli", "Omer Adam", "Noa Kirel", "Ben-El Tavori", "Itay Levi"
    };
    /// <summary>
    /// Helper record to store realistic address data for initialization.
    /// </summary>
    private readonly record struct AddressData(string Address, double Latitude, double Longitude);

    /// <summary>
    /// A list of addresses for randomizing orders.
    /// </summary>
    private static readonly AddressData[] s_addresses =
    {
        new("97 Jaffa Road, Jerusalem", 31.784321307301173, 35.21492818699034),
        new("20 King George Street, Jerusalem", 31.781445067611205, 35.21603646969099),
        new("10 Ben Yehuda Street, Jerusalem", 31.781659548628657, 35.21768334183865),
        new("30 Gaza Street, Jerusalem", 31.772801469039326, 35.21429702871634),
        new("40 Emek Refaim Street, Jerusalem", 31.76330771836013, 35.21897174033168),
        new("50 Agrippas Street, Jerusalem", 31.7839517488835, 35.2134290577043),
        new("55 Hanevi'im Street, Jerusalem", 31.784566788971983, 35.21797569918418),
        new("100 Derech Hebron, Jerusalem", 31.75372877114649, 35.220093699825775),
        new("19 King David Street, Jerusalem", 31.775532039347635, 35.22215210314104),
        new("7 Bezalel Street, Jerusalem", 31.780426988150147, 35.21376028332093),
        new("1 Shmuel HaNagid Street, Jerusalem", 31.7803234093586, 35.21509467147077),
        new("12 Hillel Street, Jerusalem", 31.780187091786093, 35.21726342807211),
        new("Mahane Yehuda Market, Jerusalem", 31.784926645982097, 35.21267841766886),
        new("The Knesset, Jerusalem", 31.776836273440203, 35.20543532782223),
        new("Israel Museum, Ruppin Boulevard, Jerusalem", 31.772260563480305, 35.20416470565706),
        new("Yad Vashem, Har Hazikaron, Jerusalem", 31.774339477464142, 35.17555929720624),
        new("Givat Ram Campus, Hebrew University, Jerusalem", 31.778454610245173, 35.19735586158905),
        new("Old City, Jaffa Gate, Jerusalem", 31.77672720366729, 35.227629940170196),
        new("Old City, Damascus Gate, Jerusalem", 31.781633499405633, 35.230407836095296),
        new("Mount Zion, Jerusalem", 31.771432755523676, 35.228548482787225)
    };

    private static void createOrders()
    {
        const int orderCount = 50;

        for (int i = 0; i < orderCount; i++)
        {
            try
            {
                string customerName = s_customerNames[s_rand.Next(s_customerNames.Length)];
                string customerPhone = $"05{s_rand.Next(10000000, 89999999)}";
                AddressData randomAddress = s_addresses[s_rand.Next(s_addresses.Length)];
                OrderType type = (OrderType)s_rand.Next(Enum.GetValues(typeof(OrderType)).Length);
                DateTime openTime = s_dalIConfig!.Clock.AddDays(-s_rand.Next(1, 60));

                Order newOrder = new()
                {
                    Id = 0,
                    OrderType = type,
                    Description = $"{type} order for {customerName}",
                    Address = randomAddress.Address,
                    Latitude = randomAddress.Latitude,
                    Longitude = randomAddress.Longitude,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    Volume = s_rand.NextDouble() > 0.5 ? s_rand.Next(1, 5) : null,
                    Weight = s_rand.NextDouble() > 0.5 ? s_rand.Next(1, 10) : null,
                    Fragile = s_rand.NextDouble() > 0.5 ? (s_rand.Next(0, 2) == 0) : null,
                    OrderTime = openTime
                };
 
                s_dalIOrder!.Create(newOrder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create order: {ex.Message}");
            }
        }

    }
    /// <summary>
    /// Creates and adds Deliveries to the database.
    /// This method links Orders to Couriers and creates the
    /// "In Progress" and "Closed" statuses.
    /// </summary>
    private static void createDeliveries()
    {
        var allOrders = s_dalIOrder!.ReadAll().ToList(); /// Get all 50 open orders
        var activeCouriers = s_dalICourier!.ReadAll().Where(c => c.IsActive).ToList();

        if (!activeCouriers.Any())
        {
            Console.WriteLine("No active couriers found, cannot create deliveries.");
            return;
        }

        int deliveriesInProgress = 10;
        int deliveriesClosed = 20;

        for (int i = 0; i < deliveriesInProgress && allOrders.Any(); i++)
        {
            /// Get random order and remove it from the available list
            int orderIndex = s_rand.Next(allOrders.Count);
            Order orderToAssign = allOrders[orderIndex];
            allOrders.RemoveAt(orderIndex); /// Ensure we don't assign it again

            Courier courier = activeCouriers[s_rand.Next(activeCouriers.Count)];

            DateTime startTime = orderToAssign.OrderTime.AddHours(s_rand.Next(1, 4));

            /// Ensure StartTime is not in the future relative to the system clock
            if (startTime > s_dalIConfig!.Clock)
                startTime = s_dalIConfig.Clock.AddMinutes(-s_rand.Next(1, 30));


            Delivery newDelivery = new()
            {
                Id = 0, /// Running ID
                OrderId = orderToAssign.Id,
                CourierId = courier.Id,
                VehicleType = courier.VehicleType, /// Vehicle type at the time of delivery
                StartTime = startTime,
                Distance = null, /// Distance is calculated in BL.
                EndOfDelivery = null, /// null = In Progress
                EndTime = null          /// null = In Progress
            };

            /// Rule: Use Interface to create (10c, 4)
            s_dalIDelivery!.Create(newDelivery);
        }

        /// Create 20 "Closed" Deliveries
        for (int i = 0; i < deliveriesClosed && allOrders.Any(); i++)
        {
            int orderIndex = s_rand.Next(allOrders.Count);
            Order orderToAssign = allOrders[orderIndex];
            allOrders.RemoveAt(orderIndex);

            Courier courier = activeCouriers[s_rand.Next(activeCouriers.Count)];

            DateTime startTime = orderToAssign.OrderTime.AddHours(s_rand.Next(1, 2));
            if (startTime > s_dalIConfig!.Clock)
                startTime = s_dalIConfig.Clock.AddHours(-s_rand.Next(2, 5));

            /// Rule: EndTime must be after StartTime (10g, 10)
            DateTime endTime = startTime.AddHours(s_rand.Next(1, 5));
            if (endTime > s_dalIConfig!.Clock)
                endTime = s_dalIConfig.Clock.AddMinutes(-s_rand.Next(1, 30));

            /// Rule: Random end type (General p. 21)
            EndOfDelivery endType = (EndOfDelivery)s_rand.Next(Enum.GetValues(typeof(EndOfDelivery)).Length);

            Delivery newDelivery = new()
            {
                Id = 0, /// Running ID
                OrderId = orderToAssign.Id,
                CourierId = courier.Id,
                VehicleType = courier.VehicleType,
                StartTime = startTime,
                Distance = null,
                EndOfDelivery = endType, /// Closed status
                EndTime = endTime        /// Closed status
            };

            s_dalIDelivery!.Create(newDelivery);
        }

        /// The remaining 20 orders (50 - 10 - 20) will stay "Open",
        /// which fulfills all data requirements.
    }

    /// <summary>
    /// Creates and adds 20 Couriers to the database.
    /// </summary>
    private static void createCouriers()
    {
        int courierCount = 20; /// As required

        for (int i = 0; i < courierCount; i++)
        {
            try
            {
                int id;
                do
                {
                    id = s_rand.Next(200000000, 400000000); /// Generate random T"Z
                }
                while (s_dalICourier!.Read(id) != null);

                string name = s_courierNames[s_rand.Next(s_courierNames.Length)];
                string phone = $"05{s_rand.Next(10000000, 99999999)}";
                string email = $"{name.Replace(" ", ".")}@company.com";

                VehicleType vehicle = (VehicleType)s_rand.Next(Enum.GetValues(typeof(VehicleType)).Length);

                bool isActive = s_rand.NextDouble() > 0.1; /// 90% chance to be active

                double? maxDistance = s_rand.NextDouble() > 0.5 ? s_rand.Next(5, 30) : null;

                DateTime startDate = s_dalIConfig!.Clock.AddDays(-s_rand.Next(30, 1000));

                Courier newCourier = new()
                {
                    Id = id,
                    Name = name,
                    PhoneNumber = phone,
                    Email = email,
                    Distance = maxDistance,
                    IsActive = isActive,
                    VehicleType = vehicle
                };

                s_dalICourier.Create(newCourier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create courier: {ex.Message}");
            }
        }
    }

    public static void Do(ICourier? dalCourier, IOrder? dalOrder, IDelivery? dalDelivery, IConfig? dalConfig)
    {
        /// Assign DAL interfaces to static fields
        s_dalICourier = dalCourier ?? throw new NullReferenceException("DAL Courier object can not be null!");
        s_dalIOrder = dalOrder ?? throw new NullReferenceException("DAL Order object can not be null!");
        s_dalIDelivery = dalDelivery ?? throw new NullReferenceException("DAL Delivery object can not be null!");
        s_dalIConfig = dalConfig ?? throw new NullReferenceException("DAL Config object can not be null!");

        /// Reset database 
        Console.WriteLine("Resetting Configuration values and Deleting all data...");
        s_dalIConfig.Reset();
        s_dalIDelivery.DeleteAll();
        s_dalIOrder.DeleteAll();
        s_dalICourier.DeleteAll();


        ///initialization
        Console.WriteLine("Initializing Couriers...");
        createCouriers();

        Console.WriteLine("Initializing Orders...");
        createOrders();

        Console.WriteLine("Initializing Deliveries...");
        createDeliveries();

        Console.WriteLine("Initialization complete.");

    }
}
