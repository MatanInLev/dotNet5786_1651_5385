using System.Collections.Generic;

namespace Dal;

/// <summary>
/// In-memory shared data store used by the DalList layer for Stage 1 testing.
/// </summary>
/// <remarks>
/// This static class exposes three public lists that act as the process-local persistence
/// for the simple DAL implementations used in the exercises:
/// <list type="bullet">
/// <item><description><see cref="Couriers"/> — holds <see cref="DO.Courier"/> records.</description></item>
/// <item><description><see cref="Orders"/> — holds <see cref="DO.Order"/> records.</description></item>
/// <item><description><see cref="Deliveries"/> — holds <see cref="DO.Delivery"/> records.</description></item>
/// </list>
///
/// Design and usage notes:
/// - The class is intentionally minimal: it only contains mutable list instances. The DAL implementations
///   perform all operations (Create/Read/Update/Delete) against these lists.
/// - The lists are process-global and shared by all DAL objects in the same AppDomain/process. Clearing a list
///   affects all consumers immediately.
/// - No invariants, transactional guarantees or referential integrity checks are enforced here.
///   Business logic and validation must be implemented in the BL or caller code.
/// - Thread-safe access is provided through lock objects. All DAL operations should use the appropriate lock
///   when accessing these lists.
/// </remarks>
/// <example>
/// Typical test harness usage:
/// <code>
/// // initialize (usually done once per test run)
/// lock (DataSource.CouriersLock) { DataSource.Couriers.Clear(); }
/// lock (DataSource.OrdersLock) { DataSource.Orders.Clear(); }
/// lock (DataSource.DeliveriesLock) { DataSource.Deliveries.Clear(); }
///
/// // DAL implementation will add entries:
/// var dalOrder = new OrderImplementation();
/// dalOrder.Create(new DO.Order { /* fields except Id */ });
/// </code>
/// </example>
internal static class DataSource
{
    /// <summary>
    /// Lock object for thread-safe access to the Couriers list.
    /// </summary>
    internal static readonly object CouriersLock = new object();

    /// <summary>
    /// Lock object for thread-safe access to the Orders list.
    /// </summary>
    internal static readonly object OrdersLock = new object();

    /// <summary>
    /// Lock object for thread-safe access to the Deliveries list.
    /// </summary>
    internal static readonly object DeliveriesLock = new object();

    /// <summary>
    /// In-memory list that stores <see cref="DO.Courier"/> instances.
    /// </summary>
    /// <remarks>
    /// Use <see cref="List{T}"/> operations or DAL wrappers (e.g., <c>CourierImplementation</c>) to mutate.
    /// Callers should not replace this field (do not assign a new list) because DAL code expects this exact reference.
    /// Always acquire CouriersLock before accessing this list.
    /// </remarks>
    internal static List<DO.Courier> Couriers = new();

    /// <summary>
    /// In-memory list that stores <see cref="DO.Order"/> instances.
    /// </summary>
    /// <remarks>
    /// The list is used as the single source-of-truth for orders within the process. DAL methods typically
    /// return shallow copies of this list to avoid exposing the internal collection reference.
    /// Always acquire OrdersLock before accessing this list.
    /// </remarks>
    internal static List<DO.Order> Orders = new();

    /// <summary>
    /// In-memory list that stores <see cref="DO.Delivery"/> instances.
    /// </summary>
    /// <remarks>
    /// Deliveries created by DAL implementations are appended to this list. Consumers that iterate this list
    /// should treat it as volatile: other code may mutate it concurrently unless external synchronization is used.
    /// Always acquire DeliveriesLock before accessing this list.
    /// </remarks>
    internal static List<DO.Delivery> Deliveries = new();
}
