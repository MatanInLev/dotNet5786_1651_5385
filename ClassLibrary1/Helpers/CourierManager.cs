
using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;

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


}
