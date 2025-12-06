namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using System;

/// <summary>
/// Internal implementation of the <see cref="IAdmin"/> interface.
/// Delegates clock manipulation, configuration access, and database
/// initialization/reset operations to the centralized <see cref="AdminManager"/>.
/// </summary>
internal class AdminImplementation : IAdmin
{
    /// <summary>
    /// Advances the application clock by one unit specified by <paramref name="unit"/>.
    /// The operation delegates to <see cref="AdminManager.UpdateClock(DateTime)"/>.
    /// </summary>
    /// <param name="unit">The time unit to advance the clock by (minutes, hours, days, months, years).</param>
    public void ForwardClock(TimeUnit unit)
    {
        switch (unit)
        {
            case TimeUnit.Minutes:
                AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
                break;
            case TimeUnit.Hours:
                AdminManager.UpdateClock(AdminManager.Now.AddHours(1));
                break;
            case TimeUnit.Days:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(1));
                break;
            case TimeUnit.Months:
                AdminManager.UpdateClock(AdminManager.Now.AddMonths(1));
                break;
            case TimeUnit.Years:
                AdminManager.UpdateClock(AdminManager.Now.AddYears(1));
                break;
        }
    }

    /// <summary>
    /// Gets the current application clock as maintained by <see cref="AdminManager"/>.
    /// </summary>
    /// <returns>The current <see cref="DateTime"/> value of the application clock.</returns>
    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    /// <summary>
    /// Retrieves the current configuration from <see cref="AdminManager"/>.
    /// </summary>
    /// <returns>The active <see cref="Config"/> instance.</returns>
    public Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    /// <summary>
    /// Initializes the in-memory or persistent database via <see cref="AdminManager.InitializeDB()"/>.
    /// </summary>
    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    /// <summary>
    /// Resets the database using <see cref="AdminManager.ResetDB()"/>.
    /// </summary>
    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    /// <summary>
    /// Replaces the current configuration with the supplied <paramref name="config"/>.
    /// Delegates to <see cref="AdminManager.SetConfig(Config)"/>.
    /// </summary>
    /// <param name="config">The new configuration to apply.</param>
    public void SetConfig(Config config)
    {
        AdminManager.SetConfig(config);
    }

    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
}
