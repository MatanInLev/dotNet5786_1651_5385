using System.Collections; 
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace BO;

static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "null";

        StringBuilder str = new StringBuilder();
        str.Append("\n");
        str.Append("Type: " + t.GetType().Name + "\n");

        foreach (PropertyInfo item in t.GetType().GetProperties())
        {
            var value = item.GetValue(t, null);

            // התיקון נמצא בשורה הבאה:
            if (value is IEnumerable && !(value is string))
            {
                str.Append(item.Name + ": \n");
                foreach (var listitem in (IEnumerable)value)
                {
                    str.Append("\t" + listitem?.ToString() + "\n");
                }
            }
            else
            {
                str.Append(item.Name + ": " + value + "\n");
            }
        }
        return str.ToString();
    }

    private const double EARTH_RADIUS_KM = 6371.0;
    /// <summary>
    /// Calculates the aerial distance (great-circle distance) between two geographic points.
    /// The calculation is based on the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point.</param>
    /// <param name="lon1">Longitude of the first point.</param>
    /// <param name="lat2">Latitude of the second point.</param>
    /// <param name="lon2">Longitude of the second point.</param>
    /// <returns>The aerial distance in kilometers (double).</returns>
    internal static double CalculateAerialDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert degrees to radians
        double lat1Rad = lat1 * (Math.PI / 180.0);
        double lon1Rad = lon1 * (Math.PI / 180.0);
        double lat2Rad = lat2 * (Math.PI / 180.0);
        double lon2Rad = lon2 * (Math.PI / 180.0);

        // Differences between the coordinates
        double deltaLat = lat2Rad - lat1Rad;
        double deltaLon = lon2Rad - lon1Rad;

        // Haversine formula calculation
        double a = Math.Sin(deltaLat / 2.0) * Math.Sin(deltaLat / 2.0) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2.0) * Math.Sin(deltaLon / 2.0);

        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        // Calculate final distance in kilometers
        return EARTH_RADIUS_KM * c;
    }


    /// <summary>
    /// Calculates the distance of a route (driving, walking, or cycling) between two coordinates
    /// by performing a synchronous network request to an external routing service.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point.</param>
    /// <param name="lon1">Longitude of the starting point.</param>
    /// <param name="lat2">Latitude of the destination point.</param>
    /// <param name="lon2">Longitude of the destination point.</param>
    /// <param name="vehicle">The vehicle type (Car, Bicycle, Foot, etc.).</param>
    /// <returns>The route distance in kilometers (double).</returns>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown on network error or parsing failure.</exception>
    internal static double CalculateRouteDistance(double lat1, double lon1, double lat2, double lon2, BO.Vehicle vehicle)
    {
        // 1. Determine the Routing Mode for the API call
        string mode;
        if (vehicle == BO.Vehicle.Car || vehicle == BO.Vehicle.Motorcycle)
        {
            // Driving and Motorcycle use the same road network route (OSRM 'driving' profile)
            mode = "driving";
        }
        else if (vehicle == BO.Vehicle.Bicycle)
        {
            // Specific profile for bicycles (OSRM 'bike' profile)
            mode = "bike";
        }
        else // BO.Vehicle.Foot (or default)
        {
            // Foot travel (OSRM 'foot' profile)
            mode = "foot";
        }

        // 2. Construct the API URL (Conceptual Example using OSRM)
        // Coordinates format: lon1,lat1;lon2,lat2
        string coordinates = $"{lon1.ToString(CultureInfo.InvariantCulture)},{lat1.ToString(CultureInfo.InvariantCulture)};" +
                             $"{lon2.ToString(CultureInfo.InvariantCulture)},{lat2.ToString(CultureInfo.InvariantCulture)}";

        // Using a public OSRM endpoint for demonstration (returns distance in meters)
        string baseUrl = "http://router.project-osrm.org/route/v1/";
        string url = $"{baseUrl}{mode}/{coordinates}?steps=false";

        // 3. Perform the Synchronous Network Request
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (dotnet-student-project)");
            var responseTask = client.GetStringAsync(url);

            try
            {
                // Force synchronous wait (Stage 4 requirement)
                responseTask.Wait();
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException("Network error while calculating route distance", ex);
            }

            string jsonContent = responseTask.Result;

            // 4. Parse the JSON Response and Extract Distance
            try
            {
                // Parsing OSRM response (distance in meters is under routes[0].distance)
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement distanceElement = doc.RootElement.GetProperty("routes")[0].GetProperty("distance");

                    // Distance is in meters, convert to kilometers
                    double distanceMeters = distanceElement.GetDouble();
                    return distanceMeters / 1000.0;
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException("Failed to parse distance from routing service response", ex);
            }
        }
    }

}