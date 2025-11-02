// Module Enums.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Document the DO enums used across the project:
/// - Provide XML documentation for each enum type and every enum member.
/// - Describe semantics, typical usage, and any special notes (units, limits, or preferred usage patterns).
/// - Keep comments in English and consistent with the existing DO types (Order, Delivery, Courier).
/// - Include a short usage example showing the enums in combination with DO records."
/// </aiPrompt>
namespace DO;

/// <summary>
/// Vehicle types used by couriers and deliveries.
/// </summary>
/// <remarks>
/// Use the most specific vehicle type appropriate for route planning, ETA estimation and capacity checks.
/// - Prefer <see cref="Car"/> or <see cref="Motorcycle"/> for longer distances or heavier loads.
/// - Use <see cref="Bike"/> or <see cref="Foot"/> for short urban trips with light payloads.
/// </remarks>
/// <example>
/// var courierVehicle = VehicleType.Bike;
/// var deliveryVehicle = VehicleType.Car;
/// </example>
public enum VehicleType
{
    /// <summary>
    /// Bicycle — typically used for short urban deliveries, small payloads.
    /// </summary>
    Bike,

    /// <summary>
    /// Car — suitable for longer trips or larger/heavier orders.
    /// </summary>
    Car,

    /// <summary>
    /// Foot — pedestrian delivery, used for very short distances or within buildings/campuses.
    /// </summary>
    Foot,

    /// <summary>
    /// Motorcycle or scooter — faster than a bike for urban routes, supports moderate payloads.
    /// </summary>
    Motorcycle
}

/// <summary>
/// High-level categories for orders. Used to influence routing, packaging and handling rules.
/// </summary>
/// <remarks>
/// These categories are intentionally coarse. Business logic may map them to internal handling rules
/// (e.g., Food -> temperature-controlled handling, Electronics -> fragile/insurance).
/// </remarks>
/// <example>
/// var type = OrderType.Food;
/// if (type == OrderType.Food) { /* apply food handling rules */ }
/// </example>
public enum OrderType
{
    /// <summary>
    /// Food deliveries (meals, restaurants).
    /// </summary>
    Food,

    /// <summary>
    /// Consumer electronics (phones, laptops, accessories).
    /// </summary>
    Electronics,

    /// <summary>
    /// Motor vehicles or vehicle-related items.
    /// </summary>
    Cars,

    /// <summary>
    /// Supermarket and grocery items.
    /// </summary>
    Groceries,

    /// <summary>
    /// Books and printed media.
    /// </summary>
    Books,

    /// <summary>
    /// Clothing and wearable items.
    /// </summary>
    Clothing,

    /// <summary>
    /// Furniture and large items requiring special handling.
    /// </summary>
    Furniture,

    /// <summary>
    /// Any other category not covered above.
    /// </summary>
    Other
}

/// <summary>
/// Final states for a delivery lifecycle. Use to mark completion reason/status.
/// </summary>
/// <remarks>
/// - Use <see cref="Delivered"/> when the recipient successfully receives the order.
/// - Use <see cref="Canceled"/> when the order was canceled before completion.
/// - Use <see cref="Failed"/> for unexpected delivery failures (system or courier error).
/// - Use <see cref="Refused"/> when the recipient explicitly refuses the delivery.
/// - Use <see cref="NotThere"/> when the recipient could not be located at the address.
/// </remarks>
/// <example>
/// delivery.EndOfDelivery = EndOfDelivery.Delivered;
/// </example>
public enum EndOfDelivery
{
    /// <summary>
    /// Delivery completed successfully and order handed to recipient.
    /// </summary>
    Delivered,

    /// <summary>
    /// Delivery was canceled (by customer, system, or merchant) before completion.
    /// </summary>
    Canceled,

    /// <summary>
    /// Delivery attempt failed due to an error (vehicle breakdown, courier unavailable, etc.).
    /// </summary>
    Failed,

    /// <summary>
    /// Recipient refused the package at delivery time.
    /// </summary>
    Refused,

    /// <summary>
    /// Recipient could not be found at the provided address/location.
    /// </summary>
    NotThere
}