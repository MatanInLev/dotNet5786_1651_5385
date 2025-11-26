namespace BO;

public enum CourierStatus
{
    None,
    Active,
    NotActive
}

public enum Vehicle
{
    None,
    Motorcycle,
    Bicycle,
    Car,
    Foot
}

public enum OrderStatus
{
    Scheduled,
    InTreatment,
    Delivered,
    CustomerRefused,
    Canceled
}

public enum OrderType
{
    Food,
    Electronics,
    Cars,
    Groceries,
    Books,
    Clothing,
    Furniture,
    Other
}

public enum DeliveryStatus
{
    None,
    InTransit,
    Delivered,
    Canceled,
    CustomerUnreachable
}

public enum ScheduleStatus
{
    OnTime,
    Late,
    Risk
}

public enum TimeUnit
{
    Minutes,
    Hours,
    Days,
    months,
    years,
}