using DalApi;
using DalList;
using DO;
using System.Runtime.CompilerServices;

namespace Dal;

/// <summary>
/// DAL implementation for managing <see cref="DO.Courier"/> entities in memory.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier.
    /// </summary>
    /// <exception cref="DalAlreadyExistsException">Thrown when courier with same ID already exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        lock (DataSource.CouriersLock)
        {
            if (DataSource.Couriers.FirstOrDefault(c => c.Id == item.Id) != null)
                throw new DalAlreadyExistsException($"An object of type courier with ID: {item.Id} already exist.");

            DataSource.Couriers.Add(item);
        }
    }

    /// <summary>
    /// Deletes the courier with the specified ID.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when courier does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        lock (DataSource.CouriersLock)
        {
            int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == id);

            if (index == -1)
                throw new DalDoesNotExistException($"An object of type courier with ID: {id} does not exist.");

            DataSource.Couriers.RemoveAt(index);
        }
    }

    /// <summary>
    /// Removes all couriers from the collection.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        lock (DataSource.CouriersLock)
        {
            DataSource.Couriers.Clear();
        }
    }

    /// <summary>
    /// Reads a single courier matching the filter condition.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        return DataSource.Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads a single courier by ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Reads all couriers, optionally filtered.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
        => filter == null
            ? DataSource.Couriers.Select(item => item)
            : DataSource.Couriers.Where(filter);

    /// <summary>
    /// Updates an existing courier.
    /// </summary>
    /// <exception cref="DalDoesNotExistException">Thrown when courier does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        lock (DataSource.CouriersLock)
        {
            int index = DataSource.Couriers.FindIndex(courier => courier.Id == item.Id);
            if (index == -1)
                throw new DalDoesNotExistException($"An object of type courier with ID: {item.Id} does not exist.");

            DataSource.Couriers[index] = item;
        }
    }
}