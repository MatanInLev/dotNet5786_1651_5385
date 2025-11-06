// Module IDelivery.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Document the DalApi interfaces for the DAL library:
/// - Add XML documentation for each interface and every member (summary, params, returns, remarks).
/// - Describe semantics, intended usage, error behaviours and any important pre/post conditions.
/// - Keep comments in English and consistent with the DO types (Order, Delivery, Courier).
/// - Embed examples where helpful to illustrate typical calling patterns."
/// </aiPrompt>
using DO;

namespace DalApi;

/// <summary>
/// Data-access interface for working with <see cref="DO.Delivery"/> entities.
/// </summary>
/// <remarks>
/// Implementations should persist Delivery records and follow the documented method semantics.
/// Typically the DAL assigns unique Ids for created deliveries and stores start/end timestamps and status.
/// </remarks>
public interface IDelivery: ICrud<Delivery>
{
    /// <summary>
    /// Creates a new <see cref="Delivery"/> record in the DAL.
    /// </summary>
    /// <param name="item">Delivery instance supplied by the caller. The DAL may assign or replace the Id.</param>
    void Create(Delivery item);

    /// <summary>
    /// Reads a delivery by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the delivery to read.</param>
    /// <returns>The stored <see cref="Delivery"/> instance when found; otherwise <c>null</c>.</returns>
    Delivery? Read(int id);

    /// <summary>
    /// Reads all deliveries currently stored by the DAL.
    /// </summary>
    /// <returns>A list of deliveries. Implementations typically return a shallow copy of the internal collection.</returns>
    List<Delivery> ReadAll();

    /// <summary>
    /// Updates an existing <see cref="Delivery"/> record.
    /// </summary>
    /// <param name="item">Delivery instance to store. The <see cref="Delivery.Id"/> property identifies the record to update.</param>
    void Update(Delivery item);

    /// <summary>
    /// Deletes the delivery with the specified identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the delivery to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all delivery records from the DAL.
    /// </summary>
    void DeleteAll();
}
