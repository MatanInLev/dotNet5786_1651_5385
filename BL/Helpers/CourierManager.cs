
using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5

    /// <summary>
    /// Periodic method to update couriers' activity status.
    /// This method is called by AdminManager whenever the system clock updates.
    /// It checks all active couriers and sets them to 'Inactive' if they haven't 
    /// performed any delivery within the configured inactivity time range.
    /// </summary>
    internal static void UpdateCourierActivityStatus()
    {
        // 1. Retrieve the inactivity time threshold from the configuration
        // According to the requirements, this is a TimeSpan (e.g., 30 days)
        TimeSpan inactivityLimit = s_dal.Config.InactivityRange;

        // 2. Get the current system time
        DateTime systemClock = AdminManager.Now;

        // 3. Retrieve all currently ACTIVE couriers from the DAL
        // We only care about active couriers; inactive ones are already handled.
        var activeCouriers = s_dal.Courier.ReadAll(c => c.IsActive);

        foreach (DO.Courier courier in activeCouriers)
        {
            // 4. Find the date of the courier's last activity.
            // Activity is defined as the EndTime of their most recent delivery.
            // If they have never made a delivery, we check their Date (Start Date).

            var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courier.Id && d.EndTime != null);

            DateTime lastActivityDate;

            if (courierDeliveries.Any())
            {
                // If deliveries exist, take the most recent EndTime
                lastActivityDate = courierDeliveries.Max(d => d.EndTime) ?? courier.Date;
            }
            else
            {
                // If no deliveries, the activity reference is the day they joined the company
                lastActivityDate = courier.Date;
            }

            // 5. Calculate the time passed since the last activity
            TimeSpan timeSinceActivity = systemClock - lastActivityDate;

            // 6. Check if the time passed exceeds the allowed inactivity limit
            if (timeSinceActivity > inactivityLimit)
            {
                // Create a new DO.Courier record with IsActive set to false (Immutable update)
                DO.Courier updatedCourier = courier with { IsActive = false };

                // Update the database
                try
                {
                    s_dal.Courier.Update(updatedCourier);
                }
                catch (Exception)
                {
                    // In a periodic background task, we usually log errors rather than throwing 
                    // them to avoid stopping the entire simulation, but for Stage 4 structure,
                    // we simply proceed.
                }
            }
        }
    }

    /// <summary>
    /// Helper method for user login (Identify only, without password).
    /// Checks if the input ID belongs to the Admin or an existing Courier.
    /// </summary>
    /// <param name="userIdInput">The user ID as a string (input from UI).</param>
    /// <returns>BO.UserRole (Admin or Courier).</returns>
    /// <exception cref="BO.BlInvalidValueException">Thrown if input is not a valid number.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the user ID does not exist.</exception>
    internal static string Login(string userIdInput)
    {
        if (!int.TryParse(userIdInput, out int userId))
        {
            throw new BO.BlInvalidValueException("User ID must be a number.");
        }

        int adminId = s_dal.Config.AdminId; 

        if (userId == adminId)
        {
            return "Admin";
        }

        DO.Courier? courier = s_dal.Courier.Read(userId);

        if (courier != null)
        {
            if (!courier.IsActive)
            {
                throw new BO.BlDoesNotExistException("Courier is no longer active.");
            }

            return "Courier";
        }

        throw new BO.BlDoesNotExistException($"User with ID {userId} does not exist in the system.");
    }

    /// <summary>
    /// Returns a list of couriers, filtered by status and vehicle type.
    /// </summary>
    /// <param name="userId">ID of the requester (Admin/Courier).</param>
    /// <param name="isActive">Filter by active status (null = all).</param>
    /// <param name="vehicle">Filter by vehicle type (null = all).</param>
    /// <returns>List of BO.CourierInList</returns>
    internal static IEnumerable<BO.CourierInList> GetCouriersList(int userId, bool? isActive = null, BO.Vehicle? vehicle = null)
    {
        // 1. Retrieve all couriers from the DAL with initial filtering at the DAL level
        var doCouriers = s_dal.Courier.ReadAll(c =>
            (isActive == null || c.IsActive == isActive) &&
            (vehicle == null || (BO.Vehicle)c.VehicleType == vehicle)
        );

        // 2. Convert (Project) from DO.Courier to BO.CourierInList
        // and calculate statistical data for each courier
        return doCouriers.Select(c =>
        {
            // Retrieve all deliveries for the current courier to calculate statistics
            var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == c.Id);

            // Check if there is an active delivery currently (no EndTime)
            var activeDelivery = courierDeliveries.FirstOrDefault(d => d.EndTime == null);

            // Calculate deliveries OnTime / Late (basic logic for demonstration)
            // Assuming MaxSupplyTime is global from configuration
            TimeSpan maxSupply = s_dal.Config.MaxDeliveryTime;

            int onTime = courierDeliveries.Count(d =>
                d.EndTime != null &&
                d.EndTime <= d.StartTime.Add(maxSupply));

            int late = courierDeliveries.Count(d =>
                d.EndTime != null &&
                d.EndTime > d.StartTime.Add(maxSupply));

            return new BO.CourierInList
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                Vehicle = (BO.Vehicle)c.VehicleType, // Casting between DO Enum and BO Enum

                // Calculated data:
                CurrentOrderId = activeDelivery?.OrderId, // If no active delivery, this will be null
                OrdersProvidedOnTime = onTime,
                OrdersProvidedLate = late
            };
        })
        .OrderBy(c => c.Id);
        }

    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    /// <param name="boCourier">The Business Object containing new courier details.</param>
    internal static void Create(BO.Courier boCourier)
    {
        // 1. Logical Validation
        if (boCourier.Id <= 0)
            throw new BO.BlInvalidValueException("Courier ID must be a positive number.");
        if (string.IsNullOrWhiteSpace(boCourier.Name))
            throw new BO.BlInvalidValueException("Courier name cannot be empty.");
        if (boCourier.MaxDistance.HasValue && boCourier.MaxDistance < 0)
            throw new BO.BlInvalidValueException("Max distance cannot be negative.");

        // 2. Check for duplicates in DAL
        // Note: ID is manual for Couriers (T.Z), so we check if it exists.
        DO.Courier? existing = s_dal.Courier.Read(boCourier.Id);
        if (existing != null)
        {
            throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.");
        }

        // 3. Map BO -> DO
        DO.Courier doCourier = new DO.Courier
        {
            Id = boCourier.Id,
            Name = boCourier.Name,
            Email = boCourier.Email,
            PhoneNumber = boCourier.Phone,
            VehicleType = (DO.VehicleType)boCourier.Vehicle, // Enum conversion
            IsActive = true, // Default to active upon creation
            Distance = boCourier.MaxDistance
            // Note: StartDate is effectively DateTime.Now via DO's default, or implied logic
        };

        // 4. Save to DAL
        try
        {
            s_dal.Courier.Create(doCourier);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.", ex);
        }
    }

    /// <summary>
    /// Retrieves full details of a specific courier.
    /// </summary>
    /// <param name="courierId">The ID of the courier to retrieve.</param>
    /// <returns>A populated BO.Courier object.</returns>
    internal static BO.Courier Get(int courierId)
    {
        // 1. Fetch from DAL
        DO.Courier? doCourier = s_dal.Courier.Read(courierId);
        if (doCourier == null)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist.");
        }

        // 2. Calculate Derived Properties
        //    Find if there is an active delivery (EndTime is null)
        var allDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId);

        //    Active Delivery:
        DO.Delivery? activeDelivery = allDeliveries.FirstOrDefault(d => d.EndTime == null);
        BO.OrderInProgress? orderInProgress = null;

        if (activeDelivery != null)
        {
            // If there is an active delivery, we need to fetch the Order details to populate OrderInProgress
            DO.Order? order = s_dal.Order.Read(activeDelivery.OrderId);
            if (order != null)
            {
                orderInProgress = new BO.OrderInProgress
                {
                    OrderId = order.Id,
                    DeliveryId = activeDelivery.Id,
                    CustomerName = order.CustomerName,
                    CustomerAddress = order.Address,
                    // Calculate status and other details via OrderManager logic if needed
                    // Status = OrderManager.CalculateOrderStatus(order.Id) 
                };
            }
        }

        // 3. Construct BO
        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Email = doCourier.Email,
            Phone = doCourier.PhoneNumber,
            Vehicle = (BO.Vehicle)doCourier.VehicleType,
            IsActive = doCourier.IsActive,
            MaxDistance = doCourier.Distance,
            // Calculated Field:
            OrderInProgress = orderInProgress
        };
    }

    /// <summary>
    /// Updates an existing courier's details.
    /// </summary>
    internal static void Update(BO.Courier boCourier)
    {
        // 1. Validate Input
        if (boCourier.Id <= 0)
            throw new BO.BlInvalidValueException("Invalid ID");

        // 2. Fetch existing to verify existence
        DO.Courier? existingCourier = s_dal.Courier.Read(boCourier.Id);
        if (existingCourier == null)
            throw new BO.BlDoesNotExistException($"Courier {boCourier.Id} was not found.");

        // 3. Business Rule: Cannot change some properties if currently delivering?
        //    (Optional rule: If handling an order, maybe limit vehicle change?)
        //    For now, we allow updates.

        // 4. Map BO -> DO (Use record 'with' for immutability if strictly following record pattern, 
        //    or create new if needed. Since DAL Update replaces the object, we create a new one based on existing).
        DO.Courier updatedCourier = existingCourier with
        {
            Name = boCourier.Name,
            Email = boCourier.Email,
            PhoneNumber = boCourier.Phone,
            VehicleType = (DO.VehicleType)boCourier.Vehicle,
            IsActive = boCourier.IsActive,
            Distance = boCourier.MaxDistance
        };

        // 5. Save to DAL
        try
        {
            s_dal.Courier.Update(updatedCourier);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Failed to update Courier {boCourier.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    internal static void Delete(int courierId)
    {
        // 1. Check Referential Integrity / Business Constraints [cite: 1657]
                    //    "Courier can be deleted on condition that he is not currently handling an order 
                    //     or never handled any order"

        IEnumerable<DO.Delivery> deliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId);

        // Check if currently handling (Active delivery)
        if (deliveries.Any(d => d.EndTime == null))
        {
            throw new BO.BlDeletionImpossibleException("Cannot delete courier: Currently handling an order.");
        }

        // Check if ever handled (History exists)
        // Depending on strictness, you might soft-delete (set IsActive=false) or prevent deletion entirely.
        // If the instruction implies "Never handled ANY order" to allow hard delete:
        if (deliveries.Any())
        {
            throw new BO.BlDeletionImpossibleException("Cannot delete courier: History of deliveries exists. Consider setting to Inactive.");
        }

        // 2. Perform Delete
        try
        {
            s_dal.Courier.Delete(courierId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier {courierId} does not exist.", ex);
        }
    }
}
