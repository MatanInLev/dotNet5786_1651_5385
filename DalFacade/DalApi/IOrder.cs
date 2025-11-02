using DO;

namespace DalApi;

/// <summary>
/// Data-access interface for working with <see cref="DO.Order"/> entities.
/// </summary>
/// <remarks>
/// Implementations should provide persistence for Order objects and follow the
/// semantics documented on individual members. Implementations are not required
/// to be thread-safe unless explicitly documented by the concrete type.
/// </remarks>
public interface IOrder
{
    /// <summary>
    /// Creates a new <see cref="Order"/> in the DAL.
    /// </summary>
    /// <param name="item">Order instance supplied by the caller. The DAL may assign or replace the Id.</param>
    /// <remarks>
    /// Implementations commonly assign a unique identifier (Id) if the caller-supplied instance
    /// contains a default placeholder Id. Validation of business rules is expected to occur in the BL;
    /// the DAL stores the supplied data as-is (unless the implementation documents additional rules).
    /// </remarks>
    void Create(Order item);

    /// <summary>
    /// Reads an <see cref="Order"/> by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the order to read.</param>
    /// <returns>The stored <see cref="Order"/> instance when found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// Many in-memory implementations return the same instance stored internally (no defensive copy).
    /// Callers that intend to modify the returned object should clone it first if they do not want to mutate DAL state.
    /// </remarks>
    Order? Read(int id);

    /// <summary>
    /// Reads all <see cref="Order"/> entities currently persisted by the DAL.
    /// </summary>
    /// <returns>A list of orders. Implementations typically return a shallow copy of the internal collection.</returns>
    /// <remarks>
    /// Use this method for enumeration and testing. For large datasets, DAL implementations may provide filtered or paged reads.
    /// </remarks>
    List<Order> ReadAll();

    /// <summary>
    /// Updates an existing <see cref="Order"/> record in the DAL.
    /// </summary>
    /// <param name="item">Order instance to store. The <see cref="Order.Id"/> property identifies the record to update.</param>
    /// <remarks>
    /// This operation is commonly a full replacement of the stored record. Implementations should throw
    /// a suitable exception (e.g., <see cref="InvalidOperationException"/>) when the target Id does not exist.
    /// </remarks>
    void Update(Order item);

    /// <summary>
    /// Deletes the order with the specified identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the order to delete.</param>
    /// <remarks>
    /// Implementations should throw an exception if the specified Id does not exist or return silently depending on the contract.
    /// </remarks>
    void Delete(int id);

    /// <summary>
    /// Deletes all orders from the DAL.
    /// </summary>
    /// <remarks>
    /// Use with caution; this operation affects all consumers of the DAL instance (in-memory shared lists, files, etc.).
    /// </remarks>
    void DeleteAll();
}
