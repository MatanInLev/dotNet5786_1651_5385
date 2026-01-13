using DalApi;
using DalList;
using DO;
using System.Runtime.CompilerServices;

namespace Dal;

/// <summary>
/// DAL implementation for managing <see cref="DO.Delivery"/> entities in memory.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new delivery with auto-generated ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Delivery item)
    {
        int newId = Config.Instance.NextDeliveryId;
        Delivery deliveryToAdd = item with { Id = newId };
        DataSource.Deliveries.Add(deliveryToAdd);
    }

    /// <summary>
    /// Deletes the delivery with the specified ID.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when delivery does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Delivery with ID: {id} does not exist.");
        DataSource.Deliveries.RemoveAt(index);
    }

    /// <summary>
    /// Removes all deliveries from the collection.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// Reads a single delivery matching the filter condition.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return DataSource.Deliveries.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads a single delivery by ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(int id)
    {
        return DataSource.Deliveries.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Reads all deliveries, optionally filtered.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
        => filter == null
            ? DataSource.Deliveries.Select(item => item)
            : DataSource.Deliveries.Where(filter);

    /// <summary>
    /// Updates an existing delivery.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when delivery does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Delivery item)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Delivery with ID: {item.Id} does not exist.");
        DataSource.Deliveries[index] = item;
    }
}