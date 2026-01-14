using DalApi;
using System.Linq;
using BO;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5
    private static AsyncMutex s_asyncMutex = new(); //stage 7
    private static AsyncMutex s_simulationMutex = new(); //stage 7

    /// <summary>
    /// Periodic method to update couriers' activity status.
    /// This method is called by AdminManager whenever the system clock updates.
    /// It checks all active couriers and sets them to 'Inactive' if they haven't 
    /// performed any delivery within the configured inactivity time range.
    /// </summary>
    internal static void UpdateCourierActivityStatus()
    {
        // Stage 7: Check if already running - if so, exit immediately
        if (s_asyncMutex.CheckAndSetInProgress())
            return;

        try
        {
            lock (AdminManager.BlMutex) //stage 7
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
            } //stage 7
        }
        finally
        {
            // Stage 7: Always release the mutex, even if an exception occurred
            s_asyncMutex.UnsetInProgress();
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
        lock (AdminManager.BlMutex) //stage 7
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
        } //stage 7
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
        lock (AdminManager.BlMutex) //stage 7
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

                // Calculate deliveries OnTime / Late
                // Count all completed deliveries (with EndTime set), regardless of final status
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
            });

            // 3. Sort results: if a vehicle type was provided, put matching vehicle couriers first,
            //    then fallback to Id ordering. Vehicle parameter is now used for ordering, not filtering.
            var ordered = projected
                .OrderByDescending(c => vehicle.HasValue && c.Vehicle == vehicle.Value)
                .ThenBy(c => c.Id);

            return ordered;
        } //stage 7
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

            lock (AdminManager.BlMutex) //stage 7
            {
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
                    Date = boCourier.StartWorkDate
                };

                // 4. Save to DAL
                try
                {
                    s_dal.Courier.Create(doCourier);
                    Logger.LogInfo($"Successfully created courier {boCourier.Id}");
                }
                catch (DO.DalAlreadyExistsException ex)
                {
                    Logger.LogError(ex, $"Courier {boCourier.Id} already exists in database");
                    throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.", ex);
                }
            } //stage 7

            // Notify observers AFTER releasing the lock to prevent deadlock
            Observers.NotifyListUpdated();
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
        lock (AdminManager.BlMutex) //stage 7
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
                    // Get configuration for distance and time calculations
                    var config = s_dal.Config;
                    
                    // Calculate distance from company to order location
                    double distance = Tools.CalculateAerialDistance(
                        config.Latitude ?? 0.0,
                        config.Longitude ?? 0.0,
                        order.Latitude,
                        order.Longitude);
                    
                    // Try to calculate actual route distance based on vehicle
                    double? actualDistance = null;
                    try
                    {
                        actualDistance = Tools.CalculateRouteDistance(
                            config.Latitude ?? 0.0,
                            config.Longitude ?? 0.0,
                            order.Latitude,
                            order.Longitude,
                            (BO.Vehicle)doCourier.VehicleType);
                    }
                    catch
                    {
                        // If routing fails, use aerial distance as actual
                        actualDistance = distance;
                    }
                    
                    // Calculate status and time information
                    BO.OrderStatus status = OrderManager.CalculateOrderStatus(order.Id);
                    BO.ScheduleStatus scheduleStatus = OrderManager.CalculateScheduleStatus(order.Id);
                    
                    TimeSpan maxTime = config.MaxDeliveryTime;
                    DateTime maxDeadline = order.OrderTime.Add(maxTime);
                    DateTime expectedDelivery = order.OrderTime.Add(maxTime / 2);
                    
                    TimeSpan timeLeft = maxDeadline - AdminManager.Now;
                    if (timeLeft < TimeSpan.Zero) timeLeft = TimeSpan.Zero;
                    
                    orderInProgress = new BO.OrderInProgress
                    {
                        OrderId = order.Id,
                        DeliveryId = activeDelivery.Id,
                        Type = (BO.OrderType)order.OrderType,
                        Description = order.Description,
                        CustomerName = order.CustomerName,
                        CustomerAddress = order.Address,
                        CustomerPhone = order.CustomerPhone,
                        Distance = distance,
                        ActualDistance = actualDistance,
                        OrderDate = order.OrderTime,
                        StartDeliveryDate = activeDelivery.StartTime,
                        ExpectedDelivery = expectedDelivery,
                        MaxDelivery = maxDeadline,
                        Status = status,
                        ScheduleStatus = scheduleStatus,
                        TimeLeft = timeLeft
                    };
                }
            }

            // Calculate on-time and late deliveries
            // Count all completed deliveries (with EndTime set), regardless of final status
            TimeSpan maxSupply = s_dal.Config.MaxDeliveryTime;

            int onTime = allDeliveries.Count(d =>
                d.EndTime != null &&
                d.EndTime <= d.StartTime.Add(maxSupply));

            int late = allDeliveries.Count(d =>
                d.EndTime != null &&
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
        } //stage 7
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

            lock (AdminManager.BlMutex) //stage 7
            {
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
                    System.Diagnostics.Debug.WriteLine($"[CourierManager] Updating courier {boCourier.Id} with name: {updatedCourier.Name}");
                    s_dal.Courier.Update(updatedCourier);
                    Logger.LogInfo($"Successfully updated courier {boCourier.Id}");
                    System.Diagnostics.Debug.WriteLine($"[CourierManager] DAL Update completed for courier {boCourier.Id}");
                } //stage 7

                // Notify observers AFTER releasing the lock to prevent deadlock
                System.Diagnostics.Debug.WriteLine($"[CourierManager] Notifying observers for courier {boCourier.Id}");
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
        lock (AdminManager.BlMutex) //stage 7
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
            // A courier has "handled" an order only if they completed a delivery (EndTime is set)
            // Deliveries that were only assigned but never completed (EndTime == null) should be removed
            if (deliveries.Any(d => d.EndTime != null))
            {
                throw new BO.BlDeletionImpossibleException("Cannot delete courier: History of deliveries exists. Consider setting to Inactive.");
            }

            // 2. Delete any assigned but never started/completed deliveries
            foreach (var delivery in deliveries)
            {
                s_dal.Delivery.Delete(delivery.Id);
            }

                // 3. Perform Delete
                try
                {
                    s_dal.Courier.Delete(courierId);
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlDoesNotExistException($"Courier {courierId} does not exist.", ex);
                }
            } //stage 7

            // Notify observers AFTER releasing the lock to prevent deadlock
            Observers.NotifyListUpdated();
    }

    /// <summary>
    /// Simulation method that automatically assigns open orders to available couriers
    /// and completes deliveries that have exceeded their expected delivery time.
    /// This method runs asynchronously as part of the Stage 7 simulator.
    /// </summary>
    internal static async Task SimulateDeliveryOperationsAsync() //stage 7
    {
        // Check if simulation is already running - if so, exit immediately
        if (s_simulationMutex.CheckAndSetInProgress())
            return;

        try
        {
            await Task.Run(() =>
            {
                lock (AdminManager.BlMutex)
                {
                    // 1. Auto-assign orders to available couriers
                    AutoAssignOrdersToCouriers();

                    // 2. Auto-complete deliveries that are overdue or ready
                    AutoCompleteDeliveries();
                }
            });
        }
        finally
        {
            s_simulationMutex.UnsetInProgress();
        }
    }

    /// <summary>
    /// Automatically assigns open orders to available couriers based on proximity and vehicle type.
    /// </summary>
    private static void AutoAssignOrdersToCouriers()
    {
        try
        {
            // Get all active couriers who are NOT currently on a delivery
            var availableCouriers = s_dal.Courier.ReadAll(c => c.IsActive)
                .Where(courier =>
                {
                    var activeDelivery = s_dal.Delivery.ReadAll(d =>
                        d.CourierId == courier.Id && d.EndTime == null);
                    return !activeDelivery.Any();
                })
                .ToList();

            if (!availableCouriers.Any())
                return; // No available couriers

            // Get all open orders (orders with no active delivery)
            var openOrders = s_dal.Order.ReadAll()
                .Where(order => OrderManager.CalculateOrderStatus(order.Id) == BO.OrderStatus.Scheduled)
                .OrderBy(o => o.OrderTime) // Oldest orders first
                .ToList();

            if (!openOrders.Any())
                return; // No orders to assign

            // Company location
            double compLat = s_dal.Config.Latitude ?? 0;
            double compLon = s_dal.Config.Longitude ?? 0;

            // Try to assign orders to couriers
            foreach (var order in openOrders)
            {
                if (!availableCouriers.Any())
                    break; // No more available couriers

                // Calculate distance to order
                double orderDistance = Tools.CalculateAerialDistance(
                    compLat, compLon, order.Latitude, order.Longitude);

                // Find a suitable courier for this order
                var suitableCourier = availableCouriers
                    .Where(c => (c.Distance ?? double.MaxValue) >= orderDistance)
                    .OrderBy(c => c.Id) // Simple ordering
                    .FirstOrDefault();

                if (suitableCourier != null)
                {
                    // Assign the order to the courier
                    DO.Delivery newDelivery = new DO.Delivery
                    {
                        Id = 0,
                        OrderId = order.Id,
                        CourierId = suitableCourier.Id,
                        StartTime = AdminManager.Now,
                        VehicleType = suitableCourier.VehicleType,
                        EndOfDelivery = null,
                        EndTime = null
                    };

                    s_dal.Delivery.Create(newDelivery);

                    // Remove courier from available list
                    availableCouriers.Remove(suitableCourier);

                    // Notify observers
                    OrderManager.Observers.NotifyItemUpdated(order.Id);
                    Observers.NotifyItemUpdated(suitableCourier.Id);

                    Logger.LogInfo($"[Simulation] Auto-assigned order {order.Id} to courier {suitableCourier.Id}");
                }
            }

            // Notify list observers once after all assignments
            if (openOrders.Any() && availableCouriers.Count < s_dal.Courier.ReadAll(c => c.IsActive).Count())
            {
                OrderManager.Observers.NotifyListUpdated();
                Observers.NotifyListUpdated();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Simulation] Error in AutoAssignOrdersToCouriers");
        }
    }

    /// <summary>
    /// Automatically completes deliveries that have been in progress for a simulated amount of time.
    /// </summary>
    private static void AutoCompleteDeliveries()
    {
        try
        {
            DateTime currentTime = AdminManager.Now;
            var config = s_dal.Config;

            // Get all active deliveries (no EndTime)
            var activeDeliveries = s_dal.Delivery.ReadAll(d => d.EndTime == null).ToList();

            foreach (var delivery in activeDeliveries)
            {
                var order = s_dal.Order.Read(delivery.OrderId);
                if (order == null)
                    continue;

                var courier = s_dal.Courier.Read(delivery.CourierId);
                if (courier == null)
                    continue;

                // Calculate expected delivery time based on distance and vehicle speed
                double distance = Tools.CalculateAerialDistance(
                    config.Latitude ?? 0,
                    config.Longitude ?? 0,
                    order.Latitude,
                    order.Longitude);

                // Get speed based on vehicle type
                double speedKmPerHour = delivery.VehicleType switch
                {
                    DO.VehicleType.Car => config.AvgCarSpeed,
                    DO.VehicleType.Motorcycle => config.AvgMotorcycleSpeed,
                    DO.VehicleType.Bike => config.AvgBicycleSpeed,
                    DO.VehicleType.Foot => config.AvgFootSpeed,
                    _ => 30.0
                };

                // Calculate estimated delivery time (distance / speed)
                double estimatedHours = distance / speedKmPerHour;
                TimeSpan estimatedTime = TimeSpan.FromHours(estimatedHours);

                // Check if delivery has been in progress long enough
                TimeSpan timeInProgress = currentTime - delivery.StartTime;

                // Complete delivery if it's been in progress for at least the estimated time
                // or if random chance (for simulation variety)
                Random random = new Random();
                bool shouldComplete = timeInProgress >= estimatedTime || 
                                     (timeInProgress >= estimatedTime * 0.5 && random.NextDouble() > 0.7);

                if (shouldComplete)
                {
                    // Randomly decide delivery outcome (most should succeed)
                    DO.EndOfDelivery outcome;
                    double outcomeChance = random.NextDouble();

                    if (outcomeChance < 0.85) // 85% delivered successfully
                        outcome = DO.EndOfDelivery.Delivered;
                    else if (outcomeChance < 0.95) // 10% customer refused
                        outcome = DO.EndOfDelivery.Refused;
                    else // 5% failed
                        outcome = DO.EndOfDelivery.Failed;

                    // Update delivery
                    var completedDelivery = delivery with
                    {
                        EndTime = currentTime,
                        EndOfDelivery = outcome,
                        Distance = distance
                    };

                    s_dal.Delivery.Update(completedDelivery);

                    // Notify observers
                    OrderManager.Observers.NotifyItemUpdated(order.Id);
                    Observers.NotifyItemUpdated(courier.Id);

                    Logger.LogInfo($"[Simulation] Auto-completed delivery {delivery.Id} for order {order.Id} with status {outcome}");
                }
            }

            // Notify list observers if any deliveries were completed
            if (activeDeliveries.Any(d =>
            {
                var updated = s_dal.Delivery.Read(d.Id);
                return updated?.EndTime != null;
            }))
            {
                OrderManager.Observers.NotifyListUpdated();
                Observers.NotifyListUpdated();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[Simulation] Error in AutoCompleteDeliveries");
        }
    }
}
