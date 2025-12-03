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
public enum OrderType
{
    /// <summary>Burgers and burger-style sandwiches.</summary>
    Burger,
    /// <summary>Pizzas and related oven-baked flatbreads.</summary>
    Pizza,
    /// <summary>Fresh salads and mixed greens.</summary>
    Salad,
    /// <summary>Sushi and Japanese-style raw or prepared seafood items.</summary>
    Sushi,
    /// <summary>Sandwiches, subs, and deli-style handhelds.</summary>
    Sandwich,
    /// <summary>Italian cuisine such as pasta, risotto, and similar dishes.</summary>
    Italian,
    /// <summary>Thai cuisine with characteristic flavors and spices.</summary>
    Thai,
    /// <summary>Indian cuisine including curries and regional specialties.</summary>
    Indian,
    /// <summary>French cuisine and related bistro-style dishes.</summary>
    French
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


/// <summary>
/// Defines the CRUD operations available in the entity sub-menus.
/// </summary>
/// <remarks>
/// The values correspond to the sub-menu options shown to the user.
/// Use TryParse to convert numeric input into this enum safely.
/// </remarks>
public enum CrudMenuOptions
{
    Return = 0,
    Create = 1,
    ReadSingle = 2,
    ReadAll = 3,
    Update = 4,
    DeleteSingle = 5,
    DeleteAll = 6
}

/// <summary>
/// Defines the options for the main application menu.
/// </summary>
/// <remarks>
/// Each enum value maps to a top-level menu action in the interactive console.
/// Use these values when parsing user input in the main loop.
/// </remarks>
public enum MainMenuOptions
{
    Exit = 0,
    Courier = 1,
    Order = 2,
    Delivery = 3,
    Configuration = 4,
    InitializeData = 5,
    ShowAllData = 6,
    ResetAllData = 7
}
