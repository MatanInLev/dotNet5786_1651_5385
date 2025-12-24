using DalApi;
using System;

namespace Dal;

internal class ConfigImplementation : IConfig
{
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    public int MaxRange
    {
        get => Config.MaxRange;
        set => Config.MaxRange = value;
    }

    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }

    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value ?? throw new ArgumentNullException(nameof(value), "CompanyAddress cannot be null.");
    }

    public double? Latitude => Config.Latitude;

    public double? Longitude => Config.Longitude;

    public double? MaxGeneralDistance
    {
        get => Config.MaxGeneralDistance;
        set => Config.MaxGeneralDistance = value ?? throw new ArgumentNullException(nameof(value), "MaxGeneralDistance cannot be null.");
    }

    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    public double AvgFootSpeed
    {
        get => Config.AvgFootSpeed;
        set => Config.AvgFootSpeed = value;
    }

    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    public TimeSpan InactivityRange
    {
        get => Config.InactivityRange;
        set => Config.InactivityRange = value;
    }

    public void Reset() => Config.Reset();
}
