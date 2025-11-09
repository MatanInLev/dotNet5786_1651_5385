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
/// - `ReadAll` returns an enumerable wrapper of the internal list.
/// - `Update` replaces the stored order that matches the supplied ID.
/// - `Delete` removes the item with the specified ID or throws <see cref="DalDoesNotExistException"/> if it does not exist.
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
internal class OrderImplementation : IOrder
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
    /// <exception cref="DalDoesNotExistException">
    /// Thrown when no order with the provided <paramref name="id"/> exists in <c>DataSource.Orders</c>.
    /// </exception>
    /// <remarks>
    /// The method locates the index of the matching order and removes it using <see cref="List{T}.RemoveAt(int)"/>.
    /// </remarks>
    public void Delete(int id)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Order with sID: {id} does not exist.");
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
    /// Reads a single order based on a provided filter condition.
    /// </summary>
    /// <param name="filter">A function to test each order for a condition.</param>
    /// <returns>
    /// The first stored <see cref="DO.Order"/> instance matching the filter; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The returned reference is the same object stored in the internal list. Modifying the returned
    /// object will affect the stored value unless the caller explicitly copies it first.
    /// </remarks>
    public Order? Read(Func<Order, bool> filter)
    {
        return DataSource.Orders.FirstOrDefault(filter);
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
        return DataSource.Orders.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    /// Reads all orders currently stored in the in-memory collection, optionally filtering them.
    /// </summary>
    /// <param name="filter">
    /// A function to test each element for a condition (optional).
    /// If <c>null</c>, all orders are returned.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{Order}"/> containing the elements from <c>DataSource.Orders</c>
    /// that pass the filter (or all elements if no filter is provided).
    /// </returns>
    /// <remarks>
    /// This method returns an enumerable wrapper around the internal collection (or a filtered view).
    /// Each <see cref="Order"/> in the returned enumerable is the same object instance from the internal list
    /// (shallow copy).
    /// </remarks>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
        => filter == null
            ? DataSource.Orders.Select(item => item)
            : DataSource.Orders.Where(filter);

    /// <summary>
    /// Updates an existing order stored in the DAL.
    /// </summary>
    /// <param name="item">
    /// The <see cref="Order"/> instance to store. The <see cref="Order.Id"/> property identifies the target record
    /// to update. All fields of the stored record are replaced with the values of <paramref name="item"/>.
    /// </param>
    /// <exception cref="DalDoesNotExistException">
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
            throw new DalDoesNotExistException($"An object of type Order with ID: {item.Id} does not exist.");
        DataSource.Orders[index] = item;
    }
}