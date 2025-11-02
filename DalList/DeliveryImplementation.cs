using DalApi;
using DalList;
using DO;

namespace Dal;

/// <summary>
/// Concrete DAL implementation for managing <see cref="DO.Delivery"/> entities in the in-memory <c>DataSource</c>.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDelivery"/> and performs simple, non-thread-safe CRUD operations against the
/// shared static collection <c>DataSource.Deliveries</c>. Behavior and design notes:
/// - <see cref="Create(Delivery)"/> assigns a new unique Id using <see cref="Config.Instance.NextDeliveryId"/> and appends
///   the resulting record to the in-memory list.
/// - <see cref="Read(int)"/> returns the stored instance (reference) or <c>null</c> when not found. Callers who modify
///   the returned object will modify the DAL's stored instance unless they clone it first.
/// - <see cref="ReadAll"/> returns a shallow copy of the internal list to avoid exposing the internal collection itself.
///   Each element in the returned list is the same instance stored internally (shallow copy semantics).
/// - <see cref="Update(Delivery)"/> replaces the stored element that matches the supplied Id. This is a full-replacement
///   operation; partial updates must be prepared by the caller before invoking Update.
/// - <see cref="Delete(int)"/> removes the element with the specified Id or throws <see cref="InvalidOperationException"/>
///   if it does not exist.
/// Thread-safety: callers must synchronize access if the DAL is used concurrently from multiple threads.
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// var dal = new DeliveryImplementation();
/// var delivery = new Delivery { /* set fields except Id */ };
/// dal.Create(delivery); // assigns Id
/// var read = dal.Read(delivery.Id);
/// var all = dal.ReadAll();
/// </code>
/// </example>
public class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new delivery record in the DAL collection.
    /// </summary>
    /// <param name="item">
    /// Caller-supplied <see cref="DO.Delivery"/> instance. The method will generate a new unique Id and
    /// create a new instance that copies all fields from <paramref name="item"/> except for the Id,
    /// which is set to the generated value.
    /// </param>
    /// <remarks>
    /// - Uses record <c>with</c> expression to preserve the caller-supplied fields while assigning the new Id.
    /// - Adds the resulting instance to <c>DataSource.Deliveries</c>.
    /// - No validation of field values is performed in this method; validation should occur prior to calling the DAL.
    /// </remarks>
    public void Create(Delivery item)
    {
        int newId = Config.Instance.NextDeliveryId;
        Delivery deliveryToAdd = item with { Id = newId };
        DataSource.Deliveries.Add(deliveryToAdd);
    }

    /// <summary>
    /// Deletes the delivery with the specified identifier from the in-memory collection.
    /// </summary>
    /// <param name="id">Unique identifier of the delivery to delete.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no delivery with the provided <paramref name="id"/> exists in <c>DataSource.Deliveries</c>.
    /// </exception>
    /// <remarks>
    /// Performs an index lookup using <see cref="List{T}.FindIndex(Predicate{T})"/> and removes the element via
    /// <see cref="List{T}.RemoveAt(int)"/>. This method mutates the shared DataSource and affects all consumers.
    /// </remarks>
    public void Delete(int id)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type Delivery with ID: {id} does not exist.");
        DataSource.Deliveries.RemoveAt(index);
    }

    /// <summary>
    /// Removes all deliveries from the in-memory collection.
    /// </summary>
    /// <remarks>
    /// This operation clears the entire <c>DataSource.Deliveries</c> list. Use with caution: it affects all consumers
    /// of the shared DataSource within the process and cannot be undone.
    /// </remarks>
    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// Reads a single delivery by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the delivery to read.</param>
    /// <returns>
    /// The stored <see cref="DO.Delivery"/> instance when found; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The returned object is the same instance that is stored in the internal list (no defensive copy).
    /// Modifying this instance will modify the DAL's stored record.
    /// </remarks>
    public Delivery? Read(int id)
    {
        Delivery? deliveryToFind = DataSource.Deliveries.Find(delivery => delivery.Id == id);
        return deliveryToFind;
    }

    /// <summary>
    /// Reads all deliveries currently stored in the in-memory collection.
    /// </summary>
    /// <returns>
    /// A new <see cref="List{Delivery}"/> containing a shallow copy of the elements from
    /// <c>DataSource.Deliveries</c> at the time of the call.
    /// </returns>
    /// <remarks>
    /// Returning a shallow copy prevents callers from mutating the internal list reference directly.
    /// However, each <see cref="DO.Delivery"/> element in the returned list is the same instance as in the internal list.
    /// </remarks>
    public List<Delivery> ReadAll()
    {
        return new List<Delivery>(DataSource.Deliveries);
    }

    /// <summary>
    /// Updates an existing delivery stored in the DAL.
    /// </summary>
    /// <param name="item">
    /// The <see cref="DO.Delivery"/> instance to store. The <see cref="DO.Delivery.Id"/> property identifies the target record
    /// to update. The stored element is replaced with the supplied <paramref name="item"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no existing delivery with <see cref="DO.Delivery.Id"/> matching <paramref name="item"/> can be found.
    /// </exception>
    /// <remarks>
    /// This method performs a full replacement of the stored element at the found index. To perform a partial update,
    /// callers should read the existing record, modify selected fields, and pass the modified record back to this method.
    /// </remarks>
    public void Update(Delivery item)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException($"An object of type Delivery with ID: {item.Id} does not exist.");
        DataSource.Deliveries[index] = item;
    }
}
