using DalApi;
using DalList;
using DO;
using System.Runtime.CompilerServices;

namespace Dal;

/// <summary>
/// DAL implementation for managing <see cref="DO.Order"/> entities in memory.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order with auto-generated ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Order item)
    {
        int newId = Config.Instance.NextOrderId;
        Order orderToAdd = item with { Id = newId };
        DataSource.Orders.Add(orderToAdd);
    }

    /// <summary>
    /// Deletes the order with the specified ID.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when order does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Order with sID: {id} does not exist.");
        DataSource.Orders.RemoveAt(index);
    }

    /// <summary>
    /// Removes all orders from the collection.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// Reads a single order matching the filter condition.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(Func<Order, bool> filter)
    {
        return DataSource.Orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads a single order by ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int id)
    {
        return DataSource.Orders.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    /// Reads all orders, optionally filtered.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
        => filter == null
            ? DataSource.Orders.Select(item => item)
            : DataSource.Orders.Where(filter);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when order does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order item)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Order with ID: {item.Id} does not exist.");
        DataSource.Orders[index] = item;
    }
}