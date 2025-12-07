namespace BO;

public enum UserRole
{
    None,
    Admin,
    Courier
}
public enum ActiveFilter
{
    All,
    Active,
    Inactive
}
/// <summary>
/// Enumeration of courier operational status.
/// </summary>
public enum CourierStatus
{
    /// <summary>Undefined / not set.</summary>
    None,
    /// <summary>Courier is active and available for assignments.</summary>
    Active,
    /// <summary>Courier is not active / unavailable.</summary>
    NotActive
}

/// <summary>
/// Supported vehicle types for couriers and deliveries.
/// </summary>
public enum Vehicle
{
    /// <summary>Undefined / not set.</summary>
    None,
    /// <summary>Motorcycle.</summary>
    Motorcycle,
    /// <summary>Bicycle.</summary>
    Bicycle,
    /// <summary>Car.</summary>
    Car,
    /// <summary>On foot.</summary>
    Foot
}

/// <summary>
/// High-level order lifecycle status.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order is scheduled / waiting for assignment.</summary>
    Scheduled,
    /// <summary>Order is currently being processed / in treatment.</summary>
    InTreatment,
    /// <summary>Order has been delivered successfully.</summary>
    Delivered,
    /// <summary>Customer refused the delivery.</summary>
    CustomerRefused,
    /// <summary>Order was canceled.</summary>
    Canceled
}

/// <summary>
/// Categories or types of orders.
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
/// Delivery result/status values.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Undefined / not set.</summary>
    None,
    /// <summary>Delivery is currently in transit.</summary>
    InTransit,
    /// <summary>Delivery completed successfully.</summary>
    Delivered,
    /// <summary>Delivery was canceled.</summary>
    Canceled,
    /// <summary>Customer could not be reached.</summary>
    CustomerUnreachable
}

/// <summary>
/// Schedule evaluation status relative to deadlines.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>On time.</summary>
    OnTime,
    /// <summary>Delivered late.</summary>
    Late,
    /// <summary>Near deadline / risk state.</summary>
    Risk
}

/// <summary>
/// Units of time used by administrative clock operations.
/// </summary>
public enum TimeUnit
{
    /// <summary>Minutes.</summary>
    Minutes,
    /// <summary>Hours.</summary>
    Hours,
    /// <summary>Days.</summary>
    Days,
    /// <summary>Months.</summary>
    Months,
    /// <summary>Years.</summary>
    Years,
}