using System.Collections; 
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace BO;

public static class Tools
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

    // Cache for route distance calculations
    // Key: (lat1, lon1, lat2, lon2, vehicle) - Value: distance in km
    private static readonly ConcurrentDictionary<(double, double, double, double, Vehicle), double> _routeDistanceCache = new();

    // Cache for geocoding (address to coordinates)
    // Key: address string - Value: (latitude, longitude)
    private static readonly ConcurrentDictionary<string, (double Latitude, double Longitude)> _geocodingCache = new();

    /// <summary>
    /// Generates a cache key for route distance calculations.
    /// Coordinates are rounded to 4 decimal places (~11 meters precision) to allow cache hits for nearby locations.
    /// </summary>
    private static (double, double, double, double, Vehicle) CreateCacheKey(double lat1, double lon1, double lat2, double lon2, Vehicle vehicle)
    {
        return (
            Math.Round(lat1, 4),
            Math.Round(lon1, 4),
            Math.Round(lat2, 4),
            Math.Round(lon2, 4),
            vehicle
        );
    }

    /// <summary>
    /// Converts an address string to geographic coordinates (latitude, longitude) using geocoding service.
    /// Results are cached to prevent duplicate network requests.
    /// </summary>
    /// <param name="address">The address to geocode.</param>
    /// <param name="progressCallback">Optional callback to report progress or wait for user (e.g., for ProgressBar or blocking).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple of (latitude, longitude).</returns>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown on network error or if address cannot be geocoded.</exception>
    public static async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string address, Action<string>? progressCallback = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BO.BlInvalidValueException("Address cannot be empty");

        // Normalize address for cache key (trim, lowercase)
        string normalizedAddress = address.Trim().ToLowerInvariant();

        // Check cache first
        if (_geocodingCache.TryGetValue(normalizedAddress, out var cachedCoords))
        {
            progressCallback?.Invoke($"Using cached coordinates for: {address}");
            return cachedCoords;
        }

        progressCallback?.Invoke($"Geocoding address: {address}...");

        // Use Nominatim (OpenStreetMap) geocoding service
        string baseUrl = "https://nominatim.openstreetmap.org/search";
        string queryParams = $"?q={Uri.EscapeDataString(address)}&format=json&limit=1";
        string url = baseUrl + queryParams;

        using (var client = new HttpClient())
        {
            // Nominatim requires User-Agent header
            client.DefaultRequestHeaders.Add("User-Agent", "DeliveryManagementSystem/1.0 (educational-project)");

            string jsonContent;
            try
            {
                progressCallback?.Invoke($"Sending request to geocoding service...");
                jsonContent = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException($"Network error while geocoding address: {address}", ex);
            }

            try
            {
                progressCallback?.Invoke($"Parsing geocoding response...");
                
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var results = doc.RootElement;
                    
                    if (results.GetArrayLength() == 0)
                    {
                        throw new BO.BlInvalidValueException($"Address not found: {address}");
                    }

                    var firstResult = results[0];
                    double latitude = firstResult.GetProperty("lat").GetString()!.ParseDouble();
                    double longitude = firstResult.GetProperty("lon").GetString()!.ParseDouble();

                    var coordinates = (latitude, longitude);
                    
                    // Cache the result
                    _geocodingCache.TryAdd(normalizedAddress, coordinates);
                    
                    progressCallback?.Invoke($"Successfully geocoded: {address} → ({latitude:F6}, {longitude:F6})");
                    
                    return coordinates;
                }
            }
            catch (BO.BlInvalidValueException)
            {
                throw; // Re-throw address not found
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException($"Failed to parse geocoding response for address: {address}", ex);
            }
        }
    }

    /// <summary>
    /// Synchronous wrapper for GetCoordinatesAsync with out parameters.
    /// Use this when you cannot use async/await.
    /// </summary>
    /// <param name="address">The address to geocode.</param>
    /// <param name="latitude">Output parameter for latitude.</param>
    /// <param name="longitude">Output parameter for longitude.</param>
    /// <param name="progressCallback">Optional callback to report progress.</param>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown on network error or if address cannot be geocoded.</exception>
    public static void GetCoordinates(string address, out double latitude, out double longitude, Action<string>? progressCallback = null)
    {
        var result = GetCoordinatesAsync(address, progressCallback).GetAwaiter().GetResult();
        latitude = result.Latitude;
        longitude = result.Longitude;
    }

    /// <summary>
    /// Helper extension method to parse double from string (handles culture-specific formats).
    /// </summary>
    private static double ParseDouble(this string value)
    {
        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Calculates the distance of a route (driving, walking, or cycling) between two coordinates
    /// by performing an asynchronous network request to an external routing service.
    /// Results are cached to prevent duplicate calculations.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point.</param>
    /// <param name="lon1">Longitude of the starting point.</param>
    /// <param name="lat2">Latitude of the destination point.</param>
    /// <param name="lon2">Longitude of the destination point.</param>
    /// <param name="vehicle">The vehicle type (Car, Bicycle, Foot, etc.).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the route distance in kilometers (double).</returns>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown on network error or parsing failure.</exception>
    public static async Task<double> CalculateRouteDistanceAsync(double lat1, double lon1, double lat2, double lon2, BO.Vehicle vehicle)
    {
        // Create cache key
        var cacheKey = CreateCacheKey(lat1, lon1, lat2, lon2, vehicle);

        // Check if value exists in cache
        if (_routeDistanceCache.TryGetValue(cacheKey, out double cachedDistance))
        {
            return cachedDistance;
        }

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

        // 3. Perform the Asynchronous Network Request
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (dotnet-student-project)");

            string jsonContent;
            try
            {
                jsonContent = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException("Network error while calculating route distance", ex);
            }

            // 4. Parse the JSON Response and Extract Distance
            try
            {
                // Parsing OSRM response (distance in meters is under routes[0].distance)
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement distanceElement = doc.RootElement.GetProperty("routes")[0].GetProperty("distance");

                    // Distance is in meters, convert to kilometers
                    double distanceMeters = distanceElement.GetDouble();
                    double distanceKm = distanceMeters / 1000.0;

                    // Store in cache before returning
                    _routeDistanceCache.TryAdd(cacheKey, distanceKm);

                    return distanceKm;
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlTemporaryNotAvailableException("Failed to parse distance from routing service response", ex);
            }
        }
    }

    /// <summary>
    /// Synchronous wrapper for CalculateRouteDistanceAsync.
    /// Use this when you cannot use async/await (e.g., in LINQ Select statements).
    /// Results are cached to prevent duplicate network requests.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point.</param>
    /// <param name="lon1">Longitude of the starting point.</param>
    /// <param name="lat2">Latitude of the destination point.</param>
    /// <param name="lon2">Longitude of the destination point.</param>
    /// <param name="vehicle">The vehicle type (Car, Bicycle, Foot, etc.).</param>
    /// <returns>The route distance in kilometers (double).</returns>
    /// <exception cref="BO.BlTemporaryNotAvailableException">Thrown on network error or parsing failure.</exception>
    public static double CalculateRouteDistance(double lat1, double lon1, double lat2, double lon2, BO.Vehicle vehicle)
    {
        return CalculateRouteDistanceAsync(lat1, lon1, lat2, lon2, vehicle).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Attempts to retrieve route distance from cache without making a network call.
    /// This is useful for UI scenarios where blocking is unacceptable.
    /// </summary>
    /// <param name="lat1">Latitude of the starting point.</param>
    /// <param name="lon1">Longitude of the starting point.</param>
    /// <param name="lat2">Latitude of the destination point.</param>
    /// <param name="lon2">Longitude of the destination point.</param>
    /// <param name="vehicle">The vehicle type.</param>
    /// <param name="distance">Output parameter for cached distance in kilometers.</param>
    /// <returns>True if cached value was found, false otherwise.</returns>
    public static bool TryGetCachedRouteDistance(double lat1, double lon1, double lat2, double lon2, BO.Vehicle vehicle, out double distance)
    {
        var cacheKey = CreateCacheKey(lat1, lon1, lat2, lon2, vehicle);
        return _routeDistanceCache.TryGetValue(cacheKey, out distance);
    }

}