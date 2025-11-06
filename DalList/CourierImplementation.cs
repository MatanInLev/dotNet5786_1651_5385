namespace Dal;
using DalApi;
using global::DalList;
using DO;
using System.Collections.Generic;

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
    /// <c>DataSource.Couriers</c> has the same <see cref="DO.Courier.Id"/>. If a courier with the same Id exists,
    /// an <see cref="InvalidOperationException"/> is thrown.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a courier with the same <see cref="DO.Courier.Id"/> already exists in the data source.
    /// </exception>
    /// <remarks>
    /// - The provided instance is appended directly to the in-memory collection; a defensive copy is not created.
    /// - The method does not auto-generate an Id; callers must supply a unique Id (or BL should generate it).
    /// </remarks>
    public void Create(Courier item)
    {
        if (DataSource.Couriers.Find(c => c.Id == item.Id) != null)
        {
            throw new InvalidOperationException($"An object of type courier with ID: {item.Id} already exist.");
        }
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Deletes the courier with the specified identifier from the in-memory collection.
    /// </summary>
    /// <param name="id">Unique identifier of the courier to delete.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no courier with the provided <paramref name="id"/> exists in <c>DataSource.Couriers</c>.
    /// </exception>
    /// <remarks>
    /// Uses <see cref="List{T}.FindIndex(Predicate{T})"/> to locate the element and removes it using
    /// <see cref="List{T}.RemoveAt(int)"/>. This mutates the shared DataSource and affects all consumers.
    /// </remarks>
    public void Delete(int id)
    {
        int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type courier with ID: {id} does not exist.");
        DataSource.Couriers.RemoveAt(index);
    }

    /// <summary>
    /// Removes all couriers from the in-memory collection.
    /// </summary>
    /// <remarks>
    /// Clears the entire <c>DataSource.Couriers</c> list. This operation is global and cannot be undone.
    /// Use only when you intend to reset the entire courier dataset for the process.
    /// </remarks>
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
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
    /// will mutate the DAL's stored instance unless the caller clones it first.
    /// </remarks>
    public Courier? Read(int id)
    {
        Courier? orderToFind = DataSource.Couriers.Find(Courier => Courier.Id == id);
        return orderToFind;
    }

    /// <summary>
    /// Reads all couriers currently stored in the in-memory collection.
    /// </summary>
    /// <returns>
    /// A new <see cref="List{Courier}"/> containing a shallow copy of the elements from <c>DataSource.Couriers</c>.
    /// </returns>
    /// <remarks>
    /// Returning a copy prevents callers from modifying the internal list reference. However, the elements inside the
    /// returned list are the same instances as stored internally (shallow copy semantics).
    /// </remarks>
    public List<Courier> ReadAll()
    {
        return new List<Courier>(DataSource.Couriers);
    }

    /// <summary>
    /// Updates an existing courier stored in the DAL.
    /// </summary>
    /// <param name="item">
    /// The <see cref="DO.Courier"/> instance to store. The <see cref="DO.Courier.Id"/> property identifies which record to update.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no existing courier with <see cref="DO.Courier.Id"/> matching <paramref name="item"/> can be found.
    /// </exception>
    /// <remarks>
    /// The method performs a full replacement of the stored element at the found index. To perform a partial update,
    /// callers should read the existing record, modify the fields they want to change, and call this method with the modified record.
    /// </remarks>
    public void Update(Courier item)
    {
        int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type courier with ID: {item.Id} does not exist.");
        DataSource.Couriers[index] = item;
    }
}
