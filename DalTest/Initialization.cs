using DalApi;
using DO;

namespace DalTest;

/// <summary>
/// Helper class that populates the DAL with realistic sample data for manual testing.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Create a set of Couriers, Orders and Deliveries in the provided DAL interfaces.
/// - Use the provided <see cref="IConfig"/> clock to create deterministic timestamps.
/// - Keep generated data realistic (addresses, names, phone numbers) and satisfy business rules:
///   * Create 20 couriers (with mostly active status).
///   * Create 50 orders (open orders initially).
///   * Create 10 deliveries in-progress and 20 closed deliveries, leaving the rest open.
/// Usage:
/// - Call <see cref="Do(IDal)"/> once to reset the DAL and populate data.
/// - This class expects the DAL interface provided to be non-null and will throw if it is null.
/// Threading:
/// - This class modifies the DAL via its interfaces; it is not thread-safe and should be used from a single thread
///   in test harness scenarios.
/// </remarks>

public static class Initialization
{
    /// <summary>
    /// Static reference to the DAL implementation, set by the 'Do' method.
    /// </summary>
    private static IDal? s_dal;

    /// <summary>
    /// Random number generator used to produce variable test data (IDs, choices, times).
    /// </summary>
    private static readonly Random s_rand = new();

    /// <summary>
    /// Customer names used by the order generator.
    /// </summary>
    private static readonly string[] s_customerNames =
    {
        "Israel Israeli", "Moshe Cohen", "Avi Levi", "Sarah Schwartz", "Rivkah Avram",
        "David Biton", "Yosef Kaplan", "Chana Berkovich", "Miriam Hadad", "Shlomo Friedman"
    };

    /// <summary>
    /// Courier names used by the courier generator.
    /// </summary>
    private static readonly string[] s_courierNames =
    {
        "Daniel Levi", "Eliran Golan", "Yair Kahana", "Ariela Levin", "Dina Klein",
        "Shir Israeli", "Omer Adam", "Noa Kirel", "Ben-El Tavori", "Itay Levi"
    };

    /// <summary>
    /// Lightweight record used to hold an address and its geographic coordinates for generating orders.
    /// </summary>
    /// <param name="Address">Human-readable address string.</param>
    /// <param name="Latitude">Latitude coordinate in decimal degrees.</param>
    /// <param name="Longitude">Longitude coordinate in decimal degrees.</param>
    private readonly record struct AddressData(string Address, double Latitude, double Longitude);

    /// <summary>
    /// A curated list of plausible addresses (with coordinates) used when creating sample orders.
    /// </summary>
    /// <remarks>
    /// Random selection from this list produces geographically consistent test data for routing or distance calculations.
    /// </remarks>
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

    /// <summary>
    /// Generate and insert a fixed number of orders into the DAL.
    /// </summary>
    /// <remarks>
    /// Implementation details and guarantees:
    /// - Creates exactly 50 orders (unless DAL.Create throws for individual orders).
    /// - Uses <see cref="IConfig.Clock"/> as the time reference and backs each order's
    ///   Open/OrderTime into the past by 1..59 days for realistic history.
    /// - Some optional numeric fields (Weight, Volume, Fragile) are randomly omitted (null) to exercise null-handling.
    /// - Each created order is passed to the DAL via <see cref="IOrder.Create(Order)"/>.
    /// Exceptions:
    /// - Exceptions thrown by DAL.Create are caught and logged to the console; generation continues for remaining orders.
    /// </remarks>
    private static void createOrders()
    {
        const int orderCount = 300;

        for (int i = 0; i < orderCount; i++)
        {
            try
            {
                string customerName = s_customerNames[s_rand.Next(s_customerNames.Length)];
                string customerPhone = $"05{s_rand.Next(10000000, 89999999)}";
                AddressData randomAddress = s_addresses[s_rand.Next(s_addresses.Length)];
                OrderType type = (OrderType)s_rand.Next(Enum.GetValues(typeof(OrderType)).Length);
                DateTime openTime = s_dal!.Config.Clock.AddDays(-s_rand.Next(1, 60));

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

                s_dal!.Order.Create(newOrder);
            }
            catch (Exception ex)
            {
                // Individual creation failures are logged; the loop continues.
                Console.WriteLine($"Failed to create order: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Create deliveries and link them to existing orders and active couriers.
    /// </summary>
    /// <remarks>
    /// Behavior and important rules enforced by this generator:
    /// - Reads all current orders via <see cref="IOrder.ReadAll"/> and uses them as candidate orders to assign.
    /// - Only couriers with <see cref="DO.Courier.IsActive"/> are considered for assignment.
    /// - Creates 10 "In Progress" deliveries (EndOfDelivery and EndTime null) and 20 "Closed" deliveries
    ///   (EndOfDelivery set and EndTime non-null).
    /// - Ensures StartTime is not in the future relative to <see cref="IConfig.Clock"/> and that EndTime &gt; StartTime.
    /// - Distance is left null because it is expected to be calculated by the business layer.
    /// - Uses <see cref="IDelivery.Create(Delivery)"/> to persist each generated delivery.
    /// Error handling:
    /// - If there are no active couriers, the method logs a message and returns early without creating deliveries.
    /// - DAL exceptions during create are propagated to the caller (they are not swallowed here).
    /// </remarks>
    private static void createDeliveries()
    {
        var allOrders = s_dal!.Order.ReadAll().ToList(); /// Get all orders
        var activeCouriers = s_dal!.Courier.ReadAll().Where(c => c.IsActive).ToList();

        if (!activeCouriers.Any())
        {
            Console.WriteLine("No active couriers found, cannot create deliveries.");
            return;
        }

        // Adjusted: fewer in-progress deliveries so not all active couriers are busy
        // With 20 active couriers, ~10 in-progress means ~50% are actively delivering
        int deliveriesInProgress = 10;  // ~50% of active couriers will have active deliveries
        int deliveriesClosed = 200;     // Increase closed to compensate, leaving ~90 open

        // Create In-Progress Deliveries (ONLY for ACTIVE couriers)
        // Use a HashSet to track which couriers already have active deliveries
        HashSet<int> couriersWithActiveDeliveries = new HashSet<int>();
        
        for (int i = 0; i < deliveriesInProgress && allOrders.Any(); i++)
        {
            /// Get random order and remove it from the available list
            int orderIndex = s_rand.Next(allOrders.Count);
            Order orderToAssign = allOrders[orderIndex];
            allOrders.RemoveAt(orderIndex); /// Ensure we don't assign it again

            // Pick a random active courier (preferably one without an active delivery already)
            Courier courier;
            int maxAttempts = 50; // Prevent infinite loop
            int attempts = 0;
            
            do
            {
                courier = activeCouriers[s_rand.Next(activeCouriers.Count)];
                attempts++;
            } 
            while (couriersWithActiveDeliveries.Contains(courier.Id) && attempts < maxAttempts);
            
            // Mark this courier as having an active delivery
            couriersWithActiveDeliveries.Add(courier.Id);

            DateTime startTime = orderToAssign.OrderTime.AddHours(s_rand.Next(1, 4));

            /// Ensure StartTime is not in the future relative to the system clock
            if (startTime > s_dal!.Config.Clock)
                startTime = s_dal!.Config.Clock.AddMinutes(-s_rand.Next(1, 30));


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
            s_dal!.Delivery.Create(newDelivery);
        }

        // Create Closed Deliveries (can use ANY courier, active or inactive)
        // Get ALL couriers (including inactive) for historical deliveries
        var allCouriersForHistory = s_dal!.Courier.ReadAll().ToList();
        
        for (int i = 0; i < deliveriesClosed && allOrders.Any(); i++)
        {
            int orderIndex = s_rand.Next(allOrders.Count);
            Order orderToAssign = allOrders[orderIndex];
            allOrders.RemoveAt(orderIndex);

            // Closed deliveries can be from any courier (even now-inactive ones who were active in the past)
            Courier courier = allCouriersForHistory[s_rand.Next(allCouriersForHistory.Count)];

            DateTime startTime = orderToAssign.OrderTime.AddHours(s_rand.Next(1, 2));
            if (startTime > s_dal!.Config.Clock)
                startTime = s_dal!.Config.Clock.AddHours(-s_rand.Next(2, 5));

            /// Rule: EndTime must be after StartTime (10g, 10)
            DateTime endTime = startTime.AddHours(s_rand.Next(1, 5));
            if (endTime > s_dal!.Config.Clock)
                endTime = s_dal!.Config.Clock.AddMinutes(-s_rand.Next(1, 30));

            /// Rule: 80% successful delivery (Delivered), 20% failed (other end types)
            EndOfDelivery endType;
            if (s_rand.NextDouble() < 0.8) // 80% chance
            {
                endType = EndOfDelivery.Delivered; // Successfully delivered
            }
            else // 20% chance of failure
            {
                // Randomly pick one of the failure types (Canceled, Failed, Refused, NotThere)
                var failureTypes = new[] { EndOfDelivery.Canceled, EndOfDelivery.Failed, EndOfDelivery.Refused, EndOfDelivery.NotThere };
                endType = failureTypes[s_rand.Next(failureTypes.Length)];
            }

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

            s_dal!.Delivery.Create(newDelivery);
        }

        /// The remaining orders will stay "Open"
    }

    /// <summary>
    /// Generate and insert a fixed number of couriers into the DAL.
    /// </summary>
    /// <remarks>
    /// - Creates 20 couriers (IDs generated randomly within a range and deduplicated against existing DAL entries).
    /// - Sets realistic names, phone numbers and emails; randomly assigns vehicle types and optionally MaxDistance.
    /// - StartDate is set relative to <see cref="IConfig.Clock"/> (random days in the past).
    /// - Exceptions from DAL operations are caught and logged; generation continues for remaining couriers.
    /// </remarks>
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
                while (s_dal!.Courier.Read(c => c.Id == id) != null);

                string name = s_courierNames[s_rand.Next(s_courierNames.Length)];
                string phone = $"05{s_rand.Next(10000000, 99999999)}";
                string email = $"{name.Replace(" ", ".")}@company.com";

                VehicleType vehicle = (VehicleType)s_rand.Next(Enum.GetValues(typeof(VehicleType)).Length);

                bool isActive = s_rand.NextDouble() > 0.1; /// 90% chance to be active

                double? maxDistance = s_rand.NextDouble() > 0.5 ? s_rand.Next(5, 30) : null;

                DateTime startDate = s_dal!.Config.Clock.AddDays(-s_rand.Next(30, 1000));

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

                s_dal!.Courier.Create(newCourier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create courier: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Main entry method that resets the DAL and populates it with sample data.
    /// </summary>
    /// <param name="dal">The DAL implementation instance to use. Must not be null.</param>
    /// <exception cref="NullReferenceException">Thrown when the provided <paramref name="dal"/> is null.</exception>
    /// <remarks>
    /// Steps performed:
    /// 1. Validate and assign the provided DAL instance (throws if null).
    /// 2. Reset configuration via <see cref="IDal.ResetDB"/> (which should delete all existing data).
    /// 3. Create couriers, then orders, then deliveries in that order to ensure referential integrity.
    /// 4. Log progress and completion to the console.
    /// This method is intended for test/setup scenarios and mutates the DAL state irreversibly for the current process.
    /// </remarks>
    public static void Do()
    {
        /// Assign DAL interfaces to static fields
        s_dal = DalApi.Factory.Get;
        /// Reset database 
        Console.WriteLine("Resetting Configuration values and Deleting all data...");
        s_dal.ResetDB();


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