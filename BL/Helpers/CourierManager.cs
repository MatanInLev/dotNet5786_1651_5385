using DalApi;
using System.Linq;

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
                // Before deactivating, cancel any active deliveries in progress
                var activeDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courier.Id && d.EndTime == null);
                foreach (var activeDelivery in activeDeliveries)
                {
                    // Close the delivery as Failed due to courier inactivity
                    var closedDelivery = activeDelivery with
                    {
                        EndTime = systemClock,
                        EndOfDelivery = DO.EndOfDelivery.Failed
                    };
                    s_dal.Delivery.Update(closedDelivery);
                }

                // Create a new DO.Courier record with IsActive set to false (Immutable update)
                DO.Courier updatedCourier = courier with { IsActive = false };

                // Update the database
                try
                {
                    s_dal.Courier.Update(updatedCourier);
                    Observers.NotifyListUpdated(); // notify PL that list changed
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
    /// <param name="userId">The user ID as an integer (input from UI).</param>
    /// <returns>BO.UserRole (Admin or Courier).</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the user ID does not exist.</exception>
    internal static BO.UserRole Login(int userId)
    {
        int adminId = s_dal.Config.AdminId;

        if (userId == adminId)
            return BO.UserRole.Admin;

        DO.Courier? courier = s_dal.Courier.Read(userId);

        if (courier != null)
        {
            if (!courier.IsActive)
                throw new BO.BlDoesNotExistException("Courier is no longer active.");

            return BO.UserRole.Courier;
        }

        throw new BO.BlDoesNotExistException($"User with ID {userId} does not exist in the system.");
    }

    /// <summary>
    /// Returns a list of couriers, filtered by status and sorted by vehicle type priority (vehicle parameter no longer filters).
    /// </summary>
    /// <param name="userId">ID of the requester (Admin/Courier).</param>
    /// <param name="isActive">Filter by active status (null = all).</param>
    /// <param name="vehicle">If provided, couriers with this vehicle type are ordered first (null = no vehicle-priority).</param>
    /// <returns>List of BO.CourierInList</returns>
    internal static IEnumerable<BO.CourierInList> GetCouriersList(int userId, bool? isActive = null, BO.Vehicle? vehicle = null)
    {
        // 1. Retrieve couriers from the DAL with filtering only on IsActive
        var doCouriers = s_dal.Courier.ReadAll(c =>
            (isActive == null || c.IsActive == isActive)
        );

        // 2. Convert (Project) from DO.Courier to BO.CourierInList
        var projected = doCouriers.Select(c =>
        {
            // Retrieve all deliveries for the current courier to calculate statistics
            var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == c.Id);

            // Check if there is an active delivery currently (no EndTime)
            var activeDelivery = courierDeliveries.FirstOrDefault(d => d.EndTime == null);

            // Calculate deliveries OnTime / Late (basic logic for demonstration)
            // Assuming MaxSupplyTime is global from configuration
            // Only count successfully delivered orders (EndOfDelivery == Delivered)
            TimeSpan maxSupply = s_dal.Config.MaxDeliveryTime;

            int onTime = courierDeliveries.Count(d =>
                d.EndTime != null &&
                d.EndOfDelivery == DO.EndOfDelivery.Delivered &&
                d.EndTime <= d.StartTime.Add(maxSupply));

            int late = courierDeliveries.Count(d =>
                d.EndTime != null &&
                d.EndOfDelivery == DO.EndOfDelivery.Delivered &&
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
        });

        // 3. Sort results: if a vehicle type was provided, put matching vehicle couriers first,
        //    then fallback to Id ordering. Vehicle parameter is now used for ordering, not filtering.
        var ordered = projected
            .OrderByDescending(c => vehicle.HasValue && c.Vehicle == vehicle.Value)
            .ThenBy(c => c.Id);

        return ordered;
    }

    /// <summary>
    /// Creates a new courier in the system (Registration).
    /// </summary>
    /// <param name="boCourier">The Business Object containing new courier details.</param>
    internal static void Create(BO.Courier boCourier)
    {
        Logger.LogInfo($"Creating new courier: {boCourier.Name} (ID: {boCourier.Id})");

        try
        {
            // 1. Validate using ValidationHelper
            ValidationHelper.ValidateCourier(boCourier);

            // 2. Check for duplicates in DAL
            DO.Courier? existing = s_dal.Courier.Read(boCourier.Id);
            if (existing != null)
            {
                Logger.LogWarning($"Courier with ID {boCourier.Id} already exists");
                throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.");
            }

            // 3. Map BO -> DO
            DO.Courier doCourier = new DO.Courier
            {
                Id = boCourier.Id,
                Name = boCourier.Name,
                Email = boCourier.Email,
                PhoneNumber = boCourier.Phone,
                VehicleType = (DO.VehicleType)boCourier.Vehicle,
                IsActive = true, // Default to active upon creation
                Distance = boCourier.MaxDistance,
                Date = AdminManager.Now
            };

            // 4. Save to DAL
            try
            {
                s_dal.Courier.Create(doCourier);
                Logger.LogInfo($"Successfully created courier {boCourier.Id}");
                Observers.NotifyListUpdated();
            }
            catch (DO.DalAlreadyExistsException ex)
            {
                Logger.LogError(ex, $"Courier {boCourier.Id} already exists in database");
                throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.", ex);
            }
        }
        catch (BO.BlBaseException)
        {
            throw; // Re-throw BL exceptions
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Unexpected error creating courier {boCourier.Id}");
            throw new BO.BlInvalidValueException($"An unexpected error occurred while creating courier {boCourier.Id}", ex);
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

        // Calculate on-time and late deliveries
        // Only count successfully delivered orders (EndOfDelivery == Delivered)
        TimeSpan maxSupply = s_dal.Config.MaxDeliveryTime;

        int onTime = allDeliveries.Count(d =>
            d.EndTime != null &&
            d.EndOfDelivery == DO.EndOfDelivery.Delivered &&
            d.EndTime <= d.StartTime.Add(maxSupply));

        int late = allDeliveries.Count(d =>
            d.EndTime != null &&
            d.EndOfDelivery == DO.EndOfDelivery.Delivered &&
            d.EndTime > d.StartTime.Add(maxSupply));

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
            StartWorkDate = doCourier.Date,
            // Calculated Fields:
            OrderInProgress = orderInProgress,
            OrdersProvidedOnTime = onTime,
            OrdersProvidedLate = late
        };
    }

    /// <summary>
    /// Updates an existing courier's details.
    /// </summary>
    internal static void Update(BO.Courier boCourier)
    {
        Logger.LogInfo($"Updating courier {boCourier.Id}");

        try
        {
            // 1. Validate using ValidationHelper
            ValidationHelper.ValidateCourier(boCourier);

            // 2. Fetch existing to verify existence
            DO.Courier? existingCourier = s_dal.Courier.Read(boCourier.Id);
            if (existingCourier == null)
            {
                Logger.LogWarning($"Courier {boCourier.Id} not found for update");
                throw new BO.BlDoesNotExistException($"Courier {boCourier.Id} was not found.");
            }

            // 3. Check if courier is being deactivated (was active, now inactive)
            bool isBeingDeactivated = existingCourier.IsActive && !boCourier.IsActive;

            // 4. If being deactivated, cancel any active deliveries
            if (isBeingDeactivated)
            {
                Logger.LogInfo($"Courier {boCourier.Id} is being deactivated, canceling active deliveries");
                var activeDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == boCourier.Id && d.EndTime == null);
                foreach (var activeDelivery in activeDeliveries)
                {
                    // Close the delivery as Canceled due to courier deactivation
                    var closedDelivery = activeDelivery with
                    {
                        EndTime = AdminManager.Now,
                        EndOfDelivery = DO.EndOfDelivery.Canceled
                    };
                    s_dal.Delivery.Update(closedDelivery);
                    
                    // Notify order observers about the cancellation
                    OrderManager.Observers.NotifyItemUpdated(activeDelivery.OrderId);
                }
                
                // Notify order list to refresh
                OrderManager.Observers.NotifyListUpdated();
            }

            // 5. Map BO -> DO
            DO.Courier updatedCourier = existingCourier with
            {
                Name = boCourier.Name,
                Email = boCourier.Email,
                PhoneNumber = boCourier.Phone,
                VehicleType = (DO.VehicleType)boCourier.Vehicle,
                IsActive = boCourier.IsActive,
                Distance = boCourier.MaxDistance
            };

            // 6. Update in DAL
            s_dal.Courier.Update(updatedCourier);
            Logger.LogInfo($"Successfully updated courier {boCourier.Id}");

            Observers.NotifyItemUpdated(boCourier.Id);
            Observers.NotifyListUpdated();
        }
        catch (BO.BlBaseException)
        {
            throw; // Re-throw BL exceptions
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Unexpected error updating courier {boCourier.Id}");
            throw new BO.BlInvalidValueException($"An unexpected error occurred while updating courier {boCourier.Id}", ex);
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
            Observers.NotifyListUpdated(); // notify PL to refresh lists after delete
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier {courierId} does not exist.", ex);
        }
    }
}
