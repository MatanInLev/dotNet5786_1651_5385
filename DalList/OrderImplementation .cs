using DalApi;
using DalList;
using DO;

namespace Dal;

/// <summary>
/// Concrete DAL implementation for managing <see cref="DO.Order"/> entities in the in-memory DataSource.
/// </summary>
/// <remarks>
/// This class implements <see cref="IOrder"/> and performs basic CRUD operations against the
/// static in-memory collection `DataSource.Orders`. Behavior notes:
/// - `Create` assigns a new unique ID using <see cref="Config.Instance.NextOrderId"/> and appends the order.
/// - `Read` returns a reference to an existing order from the collection or `null` if not found.
/// - `ReadAll` returns a shallow copy of the internal list to avoid exposing the internal collection.
/// - `Update` replaces the stored order that matches the supplied ID.
/// - `Delete` removes the item with the specified ID or throws if it does not exist.
/// Thread-safety: the implementation is not synchronized. If multiple threads access the DAL concurrently,
/// synchronization must be provided externally.
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// var dal = new OrderImplementation();
/// var order = new Order { /* set fields except Id */ };
/// dal.Create(order);
/// var read = dal.Read(order.Id);
/// </code>
/// </example>
public class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order in the DAL collection.
    /// </summary>
    /// <param name="item">
    /// The order to create. The provided instance may have default/placeholder Id; this method will
    /// assign a new unique Id obtained from <see cref="Config.Instance.NextOrderId"/> and add the resulting
    /// record to the in-memory collection.
    /// </param>
    /// <remarks>
    /// - The method uses record `with` expression to create a new instance with the generated Id,
    ///   preserving the remainder of the caller-supplied fields.
    /// - After successful completion the new order is appended to <c>DataSource.Orders</c>.
    /// - If <paramref name="item"/> has required fields missing that the business layer requires, no validation
    ///   is performed here; validation should be done before calling the DAL.
    /// </remarks>
    public void Create(Order item)
    {
        int newId = Config.Instance.NextOrderId;
        Order orderToAdd = item with { Id = newId };
        DataSource.Orders.Add(orderToAdd);
    }

    /// <summary>
    /// Deletes the order with the specified identifier from the in-memory collection.
    /// </summary>
    /// <param name="id">Unique identifier of the order to delete.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no order with the provided <paramref name="id"/> exists in <c>DataSource.Orders</c>.
    /// </exception>
    /// <remarks>
    /// The method locates the index of the matching order and removes it using <see cref="List{T}.RemoveAt(int)"/>.
    /// </remarks>
    public void Delete(int id)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type Order with sID: {id} does not exist.");
        DataSource.Orders.RemoveAt(index);
    }

    /// <summary>
    /// Removes all orders from the in-memory collection.
    /// </summary>
    /// <remarks>
    /// This operation clears the entire <c>DataSource.Orders</c> list. Use with caution:
    /// it affects all consumers of the shared DataSource within the process.
    /// </remarks>
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// Reads a single order by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the order to read.</param>
    /// <returns>
    /// The <see cref="Order"/> instance from the in-memory collection if found; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The returned reference is the same object stored in the internal list. Modifying the returned
    /// object will affect the stored value unless the caller explicitly copies it first.
    /// </remarks>
    public Order? Read(int id)
    {
        Order? orderToFind = DataSource.Orders.Find(order => order.Id == id);
        return orderToFind;
    }

    /// <summary>
    /// Reads all orders currently stored in the in-memory collection.
    /// </summary>
    /// <returns>
    /// A new <see cref="List{Order}"/> containing a shallow copy of all orders present in
    /// <c>DataSource.Orders</c> at the time of the call.
    /// </returns>
    /// <remarks>
    /// Returning a copy prevents external callers from mutating the DAL's internal list directly.
    /// Each <see cref="Order"/> in the returned list is the same object instance from the internal list
    /// (shallow copy).
    /// </remarks>
    public List<Order> ReadAll()
    {
        return new List<Order>(DataSource.Orders);
    }

    /// <summary>
    /// Updates an existing order stored in the DAL.
    /// </summary>
    /// <param name="item">
    /// The <see cref="Order"/> instance to store. The <see cref="Order.Id"/> property identifies the target record
    /// to update. All fields of the stored record are replaced with the values of <paramref name="item"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no existing order with <see cref="Order.Id"/> matching <paramref name="item"/> can be found.
    /// </exception>
    /// <remarks>
    /// The implementation finds the index of the existing order and replaces the element at that index.
    /// This is a full-replacement operation; partial updates must be performed by reading the record,
    /// updating selected fields, and then calling this method with the modified record.
    /// </remarks>
    public void Update(Order item)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type Order with ID: {item.Id} does not exist.");
        DataSource.Orders[index] = item;
    }
}
