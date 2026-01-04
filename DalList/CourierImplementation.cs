namespace Dal;
using DalApi;
using DO;
using global::DalList;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Concrete DAL implementation for managing <see cref="DO.Courier"/> entities in the in-memory <c>DataSource</c>.
/// </summary>
/// <remarks>
/// Implements <see cref="ICourier"/> and provides basic, non-thread-safe CRUD operations against the shared
/// static collection <c>DataSource.Couriers</c>. Behavior notes:
/// - <see cref="Create(Courier)"/> appends the courier if the provided Id is not already present; it throws when a duplicate Id exists.
/// - <see cref="Read(int)"/> returns the stored instance (reference) or <c>null</c> when not found.
/// - <see cref="ReadAll"/> returns a shallow copy of the internal list to avoid exposing the internal collection reference.
/// - <see cref="Update(Courier)"/> replaces the stored courier that matches the supplied Id; it throws when the target does not exist.
/// - <see cref="Delete(int)"/> removes the courier with the specified Id or throws when not found.
/// Thread-safety: callers must synchronize access if the DAL is used concurrently from multiple threads.
/// Validation: this class does minimal validation (existence checks). Field-level validation should be performed by the caller or BL.
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// var dal = new CourierImplementation();
/// var courier = new Courier { Id = 123456789, Name = \"John Doe\", Phone = \"0501234567\" };
/// dal.Create(courier);
/// var read = dal.Read(123456789);
/// var all = dal.ReadAll();
/// </code>
/// </example>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier record in the DAL collection.
    /// </summary>
    /// <param name="item">
    /// The <see cref="DO.Courier"/> instance to add. This method requires that no existing courier in
    /// <c>DataSource.Couriers</c> has the same <see cref="DO.Courier.Id"/>.
    /// </param>
    /// <exception cref="DalAlreadyExistsException">
    /// Thrown when a courier with the same <see cref="DO.Courier.Id"/> already exists in the data source.
    /// </exception>
    /// <remarks>
    /// - The provided instance is appended directly to the in-memory collection; a defensive copy is not created.
    /// - The method does not auto-generate an Id; callers must supply a unique Id.
    /// - Thread-safe: uses DataSource.CouriersLock for synchronization.
    /// </remarks>
    public void Create(Courier item)
    {
        lock (DataSource.CouriersLock)
        {
            if (DataSource.Couriers.FirstOrDefault(c => c.Id == item.Id) != null)
            {
                throw new DalAlreadyExistsException($"An object of type courier with ID: {item.Id} already exist.");
            }
            DataSource.Couriers.Add(item);
        }
    }

    /// <summary>
    /// Deletes the courier with the specified identifier using LINQ.
    /// (Updated for Stage 2, Chapter 8 & 9)
    /// </summary>
    /// <param name="id">Unique identifier of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">
    /// Thrown when no courier with the provided id exists.
    /// </exception>
    /// <remarks>
    /// Thread-safe: uses DataSource.CouriersLock for synchronization.
    /// </remarks>
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
    /// Removes all couriers from the in-memory collection.
    /// </summary>
    /// <remarks>
    /// Clears the entire <c>DataSource.Couriers</c> list. This operation is global and cannot be undone.
    /// Use only when you intend to reset the entire courier dataset for the process.
    /// Thread-safe: uses DataSource.CouriersLock for synchronization.
    /// </remarks>
    public void DeleteAll()
    {
        lock (DataSource.CouriersLock)
        {
            DataSource.Couriers.Clear();
        }
    }

    /// <summary>
    /// Reads a single courier based on a provided filter condition.
    /// </summary>
    /// <param name="filter">A function to test each courier for a condition.</param>
    /// <returns>
    /// The first stored <see cref="DO.Courier"/> instance matching the filter; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The returned reference points to the same object stored in the internal list. Modifying the returned object
    /// will mutate the DAL's stored instance unless the caller clones it first.
    /// </remarks>
    public Courier? Read(Func<Courier, bool> filter)
    {
        return DataSource.Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads a single courier by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the courier to read.</param>
    /// <returns>
    /// The stored <see cref="DO.Courier"/> instance when found; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The returned reference points to the same object stored in the internal list. Modifying the returned object
    /// will mutate the DAL's stored instance.
    /// </remarks>
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Reads all couriers currently stored in the in-memory collection, optionally filtering them.
    /// </summary>
    /// <param name="filter">
    /// A function to test each element for a condition (optional).
    /// If <c>null</c>, all couriers are returned.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{Courier}"/> containing the elements from <c>DataSource.Couriers</c> that pass the filter.
    /// </returns>
    /// <remarks>
    /// This method returns an enumerable wrapper around the internal collection (or a filtered view).
    /// The elements returned are the same instances as stored internally (shallow copy semantics).
    /// </remarks>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
        => filter == null
            ? DataSource.Couriers.Select(item => item)
            : DataSource.Couriers.Where(filter);


    /// <summary>
    /// Updates an existing courier stored in the DAL.
    /// </summary>
    /// <param name="item">
    /// The <see cref="DO.Courier"/> instance to store. The <see cref="DO.Courier.Id"/> property identifies which record to update.
    /// </param>
    /// <exception cref="DalDoesNotExistException">
    /// Thrown when no existing courier with the <see cref="DO.Courier.Id"/> matching <paramref name="item"/> can be found.
    /// </exception>
    /// <remarks>
    /// The method finds the index of the existing courier and performs a full replacement of the stored element at that index.
    /// To perform a partial update, callers should read the existing record, modify the fields, and then call this method.
    /// </remarks>
    public void Update(Courier item)

    {
        int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type courier with ID: {item.Id} does not exist.");
        DataSource.Couriers[index] = item;

    }
}