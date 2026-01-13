using DalApi;
using System;
using System.Runtime.CompilerServices;

namespace DalList;

/// <summary>
/// Holds global configuration settings and ID counters for the DalList in-memory DAL.
/// </summary>
/// <remarks>
/// This class is implemented as a process-local singleton (see <see cref="Instance"/>).
/// It centralizes configuration values used by DAL implementations and the test harness,
/// including:
/// - auto-incrementing ID counters for orders and deliveries (<see cref="NextOrderId"/>, <see cref="NextDeliveryId"/>),
/// - a system clock for deterministic test runs (<see cref="Clock"/>),
/// - global static defaults for speeds and time ranges,
/// - a reset facility (<see cref="Reset"/>) that restores defaults and counters.
/// 
/// Important behavior notes:
/// - <see cref="NextOrderId"/> and <see cref="NextDeliveryId"/> return the next value and increment the internal counter
///   on each get access. Callers should retrieve the value once when creating a new entity.
/// - Many settings are implemented as static members (shared across the AppDomain). Instance members are accessible
///   via the singleton <see cref="Instance"/>. The separation exists to allow an instance-based proxy
///   (<c>ConfigImplementation</c>) to map instance properties while keeping some defaults globally available.
/// - This class is NOT thread-safe. If your application uses multiple threads you must synchronize access externally.
/// </remarks>
internal class Config
{
    /// <summary>
    /// Singleton instance providing instance-scoped members and reset behaviour.
    /// </summary>
    internal static Config Instance { [MethodImpl(MethodImplOptions.Synchronized)] get; } = new();

    // Prevent external construction
    private Config() { }

    // --- Auto-incrementing ID counters ---

    private const int OrderStartId = 1;
    private int _nextOrderId = OrderStartId;

    /// <summary>
    /// Returns the next unique order identifier and increments the internal counter.
    /// </summary>
    /// <remarks>
    /// Use this property when creating a new order. The call returns a unique integer and
    /// advances the internal counter by one. The counter is reset to <c>OrderStartId</c> by <see cref="Reset"/>.
    /// </remarks>
    internal int NextOrderId 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _nextOrderId++; 
    }

    private const int DeliveryStartId = 1;
    private int _nextDeliveryId = DeliveryStartId;

    /// <summary>
    /// Returns the next unique delivery identifier and increments the internal counter.
    /// </summary>
    /// <remarks>
    /// Use this property when creating a new delivery. The call returns a unique integer and
    /// advances the internal counter by one. The counter is reset to <c>DeliveryStartId</c> by <see cref="Reset"/>.
    /// </remarks>
    internal int NextDeliveryId 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _nextDeliveryId++; 
    }

    // --- Instance-scoped configuration members ---

    private DateTime _clock = DateTime.Now;
    /// <summary>
    /// A configurable system clock used for deterministic tests and simulated time.
    /// </summary>
    /// <remarks>
    /// This value is instance-scoped (accessible through <see cref="Instance"/>). The test harness
    /// sets the clock to a fixed past time to produce deterministic initialization data.
    /// </remarks>
    internal DateTime Clock 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _clock;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _clock = value;
    }

    private int _maxRange = 50;
    /// <summary>
    /// Maximum range value used by business rules (instance-scoped).
    /// </summary>
    /// <remarks>
    /// This property is exposed on the <see cref="Config"/> instance so that the implementing proxy
    /// (<c>ConfigImplementation</c>) can map instance access to callers implementing <see cref="DalApi.IConfig"/>.
    /// </remarks>
    public int MaxRange 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _maxRange;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _maxRange = value;
    }

    private static int _adminId = 123456782;
    /// <summary>
    /// The admin's official ID (T.Z.). Shared static default.
    /// </summary>
    internal static int AdminId 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _adminId;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _adminId = value;
    }

    private static string? _companyAddress = null;
    /// <summary>The company's base address. Null indicates not set.</summary>
    internal static string? CompanyAddress 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _companyAddress;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _companyAddress = value;
    }

    private static double? _latitude = null;
    /// <summary>The latitude of the company base, if available.</summary>
    internal static double? Latitude 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _latitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _latitude = value;
    }

    private static double? _longitude = null;
    /// <summary>The longitude of the company base, if available.</summary>
    internal static double? Longitude 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _longitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _longitude = value;
    }

    private static double? _maxGeneralDistance = null;
    /// <summary>Max allowed aerial distance (km) for deliveries; null = no restriction.</summary>
    internal static double? MaxGeneralDistance 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _maxGeneralDistance;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _maxGeneralDistance = value;
    }

    private static double _avgCarSpeed = 60;
    /// <summary>Average vehicle speeds used for estimations (km/h): car.</summary>
    internal static double AvgCarSpeed 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _avgCarSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _avgCarSpeed = value;
    }

    private static double _avgMotorcycleSpeed = 70;
    /// <summary>Average vehicle speeds used for estimations (km/h): motorcycle.</summary>
    internal static double AvgMotorcycleSpeed 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _avgMotorcycleSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _avgMotorcycleSpeed = value;
    }

    private static double _avgBicycleSpeed = 20;
    /// <summary>Average vehicle speeds used for estimations (km/h): bicycle.</summary>
    internal static double AvgBicycleSpeed 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _avgBicycleSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _avgBicycleSpeed = value;
    }

    private static double _avgFootSpeed = 5;
    /// <summary>Average walking speed used for estimations (km/h): foot.</summary>
    internal static double AvgFootSpeed 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _avgFootSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _avgFootSpeed = value;
    }

    private static TimeSpan _maxDeliveryTime = TimeSpan.FromHours(2);
    /// <summary>The maximum delivery time the company commits to (default: 2 hours).</summary>
    internal static TimeSpan MaxDeliveryTime 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _maxDeliveryTime;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _maxDeliveryTime = value;
    }

    private static TimeSpan _riskRange = TimeSpan.FromMinutes(15);
    /// <summary>Time before the maximum delivery time when an order is considered at risk (default: 15 minutes).</summary>
    internal static TimeSpan RiskRange 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _riskRange;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _riskRange = value;
    }

    private static TimeSpan _inactivityRange = TimeSpan.FromDays(3);
    /// <summary>Inactivity range after which a courier is considered inactive (default: 3 days).</summary>
    internal static TimeSpan InactivityRange 
    { 
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _inactivityRange;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => _inactivityRange = value;
    }


    /// <summary>
    /// Resets all configuration properties and ID counters to their initial defaults.
    /// </summary>
    /// <remarks>
    /// - Resets instance counters (<see cref="_nextOrderId"/>, <see cref="_nextDeliveryId"/>) and <see cref="Clock"/>.
    /// - Restores all static default fields (speeds, ranges, admin details, etc.) to their original values.
    /// - This operation affects all users of the shared static fields and should be used only in test/setup scenarios.
    /// </remarks>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Reset()
    {
        // Reset counters and instance members
        _nextOrderId = OrderStartId;
        _nextDeliveryId = DeliveryStartId;
        _clock = DateTime.Now;
        _maxRange = 50;

        // Reset static defaults
        _adminId = 123456782;
        _companyAddress = "Machon Lev";
        _latitude = 31.765847796216843;
        _longitude = 35.19104519946595;
        _maxGeneralDistance = null;
        _avgCarSpeed = 60;
        _avgMotorcycleSpeed = 70;
        _avgBicycleSpeed = 20;
        _avgFootSpeed = 5;
        _maxDeliveryTime = TimeSpan.FromHours(2);
        _riskRange = TimeSpan.FromMinutes(15);
        _inactivityRange = TimeSpan.FromDays(3);
    }
}