using DO;

namespace DalApi;

/// <summary>
/// Data-access interface for working with <see cref="DO.Courier"/> entities.
/// </summary>
/// <remarks>
/// Implementations should persist courier records and expose simple CRUD operations.
/// The interface intentionally mirrors other DAL entity interfaces to keep usage consistent across entities.
/// </remarks>
public interface ICourier
{
    /// <summary>
    /// Creates a new <see cref="Courier"/> record in the DAL.
    /// </summary>
    /// <param name="item">Courier instance supplied by the caller. The DAL may assign or replace the Id.</param>
    /// <remarks>
    /// Implementations typically ensure the returned/created record has a unique Id.
    /// </remarks>
    void Create(Courier item);

    /// <summary>
    /// Reads a courier by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the courier to read.</param>
    /// <returns>The stored <see cref="Courier"/> instance when found; otherwise <c>null</c>.</returns>
    Courier? Read(int id);

    /// <summary>
    /// Reads all courier records from the DAL.
    /// </summary>
    /// <returns>A list of couriers. Implementations typically return a shallow copy of the internal collection.</returns>
    List<Courier> ReadAll();

    /// <summary>
    /// Updates an existing <see cref="Courier"/> record.
    /// </summary>
    /// <param name="item">Courier instance to store; <see cref="Courier.Id"/> identifies the record to update.</param>
    void Update(Courier item);

    /// <summary>
    /// Deletes the courier with the specified identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the courier to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all courier records from the DAL.
    /// </summary>
    void DeleteAll();
}
