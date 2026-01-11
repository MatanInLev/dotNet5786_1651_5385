using BO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4
    
    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    private static Task? _periodicTask = null; //stage 7
    private static Task? _simulateTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        DateTime oldClock;
        lock (BlMutex)
        {
            oldClock = s_dal.Config.Clock;
            s_dal.Config.Clock = newClock;
        }

        // Schedule periodic updates asynchronously (stage 7)
        if (_periodicTask is null || _periodicTask.IsCompleted)
            _periodicTask = PeriodicUpdatesAsync(oldClock, newClock);

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    private static Task PeriodicUpdatesAsync(DateTime oldClock, DateTime newClock)
    {
        return Task.Run(() =>
        {
            lock (BlMutex)
            {
                // Periodic logic that must run after each clock tick
                CourierManager.UpdateCourierActivityStatus();
            }
        });
    }

    private static Task SimulateOperationsAsync()
    {
        return Task.Run(() =>
        {
            lock (BlMutex)
            {
                // TODO: add domain-specific simulation (add/update/delete) if required
            }
        });
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
    => new BO.Config()
    {
        MaxRange = s_dal.Config.MaxRange,
        CompanyAddress = s_dal.Config.CompanyAddress,
        CarSpeed = s_dal.Config.AvgCarSpeed,
        MotorcycleSpeed = s_dal.Config.AvgMotorcycleSpeed,
        BicycleSpeed = s_dal.Config.AvgBicycleSpeed,
        FootSpeed = s_dal.Config.AvgFootSpeed,
        RiskRange = s_dal.Config.RiskRange,
        InactivityTime = s_dal.Config.InactivityRange,
        MaxDeliveryTime = s_dal.Config.MaxDeliveryTime,
        AdminId = s_dal.Config.AdminId
    };

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        bool configChanged = false; // stage 5

        if (s_dal.Config.MaxRange != configuration.MaxRange) //stage 4
        {
            s_dal.Config.MaxRange = configuration.MaxRange;
            configChanged = true;
        }
        if (s_dal.Config.CompanyAddress != configuration.CompanyAddress)
        {
            s_dal.Config.CompanyAddress = configuration.CompanyAddress;
            configChanged = true;
        }

        if (s_dal.Config.AvgCarSpeed != configuration.CarSpeed)
        {
            s_dal.Config.AvgCarSpeed = configuration.CarSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgMotorcycleSpeed != configuration.MotorcycleSpeed)
        {
            s_dal.Config.AvgMotorcycleSpeed = configuration.MotorcycleSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgBicycleSpeed != configuration.BicycleSpeed)
        {
            s_dal.Config.AvgBicycleSpeed = configuration.BicycleSpeed;
            configChanged = true;
        }

        if (s_dal.Config.AvgFootSpeed != configuration.FootSpeed)
        {
            s_dal.Config.AvgFootSpeed = configuration.FootSpeed;
            configChanged = true;
        }

        if (s_dal.Config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }

        if (s_dal.Config.InactivityRange != configuration.InactivityTime)
        {
            s_dal.Config.InactivityRange = configuration.InactivityTime;
            configChanged = true;
        }

        if (s_dal.Config.MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            ConfigUpdatedObservers?.Invoke(); //stage 5 - needed for update the PL
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            if (_simulateTask is null || _simulateTask.IsCompleted)
                _simulateTask = SimulateOperationsAsync();

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}
