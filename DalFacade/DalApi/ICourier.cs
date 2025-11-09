// Module ICourier.cs
/// <aiPrompt>
/// AI PROMPT (for documentation):
/// "Document the DalApi courier interface briefly:
/// - Short summary and one-line remarks describing semantics.
/// - Keep examples minimal and comments in English."
/// </aiPrompt>
using DO;

namespace DalApi;

/// <summary>
/// CRUD operations for <see cref="DO.Courier"/> records.
/// </summary>
/// <remarks>
/// Implementations persist courier records; Create may assign the <see cref="DO.Courier.Id"/>.
/// Read/ReadAll commonly return stored instances (no defensive copy). Update/Delete should signal missing Ids via exceptions.
/// </remarks>
/// <example>
/// <code>ICourier dal = GetCourierDal(); dal.Create(new Courier { Id = 0, Name = "A" });</code>
/// </example>
public interface ICourier : ICrud<Courier> { }
