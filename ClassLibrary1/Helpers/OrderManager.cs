

using DalApi;
using System.Globalization;
using System.Xml.Linq;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;


    /// <summary>
    /// Calculates the schedule status (OnTime, InRisk, Late) of a specific order.
    /// The calculation is based on the order creation time, the maximum supply time configuration,
    /// the risk range configuration, and the system clock.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The calculated ScheduleStatus (OnTime, InRisk, or Late).</returns>
    internal static BO.ScheduleStatus CalculateScheduleStatus(int orderId)
    {
        // 1. Retrieve the Order entity from the DAL
        DO.Order? order = s_dal.Order.Read(orderId);

        // 2. Retrieve configuration variables for time limits
        // Note: Assuming these properties exist in IConfig as per the requirements
        TimeSpan maxSupplyTime = s_dal.Config.MaxDeliveryTime;
        TimeSpan riskRange = s_dal.Config.RiskRange;

        // 3. Calculate deadline and risk threshold times
        DateTime orderCreationTime = order.OrderTime;
        DateTime maxDeadline = orderCreationTime.Add(maxSupplyTime);
        DateTime riskThreshold = maxDeadline.Subtract(riskRange);

        // 4. Retrieve the relevant delivery information (if exists)
        // We look for the latest delivery attempt associated with this order
        var allDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId);
        DO.Delivery? lastDelivery = allDeliveries
                                    .OrderByDescending(d => d.StartTime)
                                    .FirstOrDefault();

        // 5. Determine if the order is effectively "Closed" (Delivered, Refused, or Canceled)
        bool isClosed = false;
        DateTime? actualEndTime = null;

        if (lastDelivery != null && lastDelivery.EndTime != null)
        {
            // Check if the delivery ended in a state that closes the order
            if (lastDelivery.EndOfDelivery == DO.EndOfDelivery.Delivered ||
                lastDelivery.EndOfDelivery == DO.EndOfDelivery.Refused ||
                lastDelivery.EndOfDelivery == DO.EndOfDelivery.Canceled)
            {
                isClosed = true;
                actualEndTime = lastDelivery.EndTime;
            }
        }

        // 6. Calculate Status based on the System Clock (AdminManager.Now)
        DateTime systemClock = AdminManager.Now;

        // Logic Scenario A: The order is Closed
        if (isClosed && actualEndTime.HasValue)
        {
            // If finished after the max deadline -> Late
            if (actualEndTime.Value > maxDeadline)
                return BO.ScheduleStatus.Late;

            // If finished before the deadline (even if it was inside the risk range) -> OnTime
            return BO.ScheduleStatus.OnTime;
        }
        // Logic Scenario B: The order is Open or InProgress
        else
        {
            // If current time passed the deadline -> Late
            if (systemClock > maxDeadline)
                return BO.ScheduleStatus.Late;

            // If current time passed the risk threshold but not the deadline -> InRisk
            if (systemClock >= riskThreshold)
                return BO.ScheduleStatus.Risk;

            // Otherwise -> OnTime
            return BO.ScheduleStatus.OnTime;
        }
    }
    /// <summary>
    /// Helper method to calculate coordinates (Latitude, Longitude) for a given address.
    /// Performs a synchronous network request to a Geocoding API (e.g., OpenStreetMap Nominatim).
    /// Note: This method is synchronous as required for Stage 4.
    /// </summary>
    /// <param name="address">The physical address to search for.</param>
    /// <param name="latitude">Output: The calculated latitude.</param>
    /// <param name="longitude">Output: The calculated longitude.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the address is invalid or not found.</exception>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown if there is a network or parsing error.</exception>
    internal static void GetCoordinates(string address, out double latitude, out double longitude)
    {
        // 1. API URL definition (Using OpenStreetMap's Nominatim as a free example)
        // polygon=1 and addressdetails=1 provide extra info if needed, format=xml ensures XML response.
        string url = $"https://nominatim.openstreetmap.org/search?q={address}&format=xml&polygon=1&addressdetails=1";

        // 2. Synchronous network request (Adapted for Stage 4 requirements)
        using (var client = new HttpClient())
        {
            // Nominatim requires a valid User-Agent header
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (dotnet-student-project)");

            var responseTask = client.GetStringAsync(url);

            try
            {
                // Force synchronous wait to comply with Stage 4 requirements (no async/await yet)
                responseTask.Wait();
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException("Network error while trying to get coordinates", ex);
            }

            string xmlContent = responseTask.Result;

            // 3. XML Parsing and extraction of coordinates
            XElement root;
            try
            {
                // Wrap content in a root element to handle multiple results or empty responses safely
                root = XElement.Parse("<root>" + xmlContent + "</root>");
            }
            catch
            {
                throw new BO.BlTemporaryNotAvailableException("Invalid response from Geocoding server");
            }

            // Get the first 'place' element found
            XElement? place = root.Elements("place").FirstOrDefault();

            // 4. Validation: Check if the address was found
            if (place == null)
            {
                throw new BO.BlDoesNotExistException($"Address not found: {address}");
            }

            // Extract 'lat' and 'lon' attributes and parse them to double
            // Using NumberStyles.Any and InvariantCulture to handle different decimal separators
            if (double.TryParse(place.Attribute("lat")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                double.TryParse(place.Attribute("lon")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                latitude = lat;
                longitude = lon;
            }
            else
            {
                throw new BO.BlTemporaryNotAvailableException("Failed to parse coordinates from server response");
            }
        }
    }

    /// <summary>
    /// Helper method to calculate the current status of an order (Scheduled, InTreatment, Delivered, etc.).
    /// The status is determined based on the history of deliveries associated with the order, 
    /// specifically focusing on the most recent delivery attempt.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The calculated BO.OrderStatus.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown when the order ID does not exist in the DAL.</exception>
    internal static BO.OrderStatus CalculateOrderStatus(int orderId)
    {
        // 1. Verify the order exists in the DAL
        DO.Order? order = s_dal.Order.Read(orderId);
        if (order == null)
        {
            throw new BO.BlDoesNotExistException($"Order with ID {orderId} does not exist");
        }

        // 2. Retrieve all deliveries associated with this order
        var allDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId);

        // 3. If there are no deliveries associated with the order, it is definitively "Scheduled" (Open)
        if (!allDeliveries.Any())
        {
            return BO.OrderStatus.Scheduled;
        }

        // 4. Retrieve the latest delivery attempt based on start time.
        // The current status depends on the most recent action taken.
        DO.Delivery lastDelivery = allDeliveries
                                   .OrderByDescending(d => d.StartTime)
                                   .First();

        // 5. Check if the latest delivery is still active (EndTime is null)
        // If so, the order is currently being handled by a courier.
        if (lastDelivery.EndTime == null)
        {
            return BO.OrderStatus.InTreatment;
        }

        // 6. The delivery has ended. Determine the status based on the result.
        if (lastDelivery.EndOfDelivery == null)
        {
            // Fallback in case result is null but time is set (should not happen in valid flow)
            return BO.OrderStatus.Scheduled;
        }

        switch (lastDelivery.EndOfDelivery)
        {
            case DO.EndOfDelivery.Delivered:
                return BO.OrderStatus.Delivered;

            case DO.EndOfDelivery.Refused:
                return BO.OrderStatus.CustomerRefused;

            case DO.EndOfDelivery.Canceled:
                return BO.OrderStatus.Canceled;

            // Special Cases: Technical failure or Customer Not Found.
            // In these cases, the specific delivery failed, but the Order itself 
            // returns to the pool to be picked up by another courier.
            case DO.EndOfDelivery.NotThere:
            case DO.EndOfDelivery.Failed:
                return BO.OrderStatus.Scheduled;

            default:
                return BO.OrderStatus.Scheduled;
        }
    }
}
