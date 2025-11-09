// Module IDelivery.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Document the DalApi delivery interface briefly:
/// - Add a short summary and a one-line remark.
/// - Keep examples minimal and comments in English."
/// </aiPrompt>
using DO;

namespace DalApi;

/// <summary>
/// CRUD operations for <see cref="DO.Delivery"/> records.
/// </summary>
/// <remarks>
/// Implementations persist deliveries; Create may assign the Id. Read/ReadAll commonly return stored instances (no defensive copy).
/// Update/Delete should signal missing Ids via exceptions (e.g., <see cref="InvalidOperationException"/>).
/// </remarks>
/// <example>
/// <code>IDelivery dal = GetDeliveryDal(); dal.Create(new Delivery { OrderId=1, CourierId=2 });</code>
/// </example>
public interface IDelivery : ICrud<Delivery> { }
