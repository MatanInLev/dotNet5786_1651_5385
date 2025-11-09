using DO;

namespace DalApi;

/// <summary>
/// Data-access interface for working with <see cref="DO.Order"/> entities.
/// </summary>
/// <remarks>
/// Implementations should provide persistence for Order objects and follow the
/// semantics documented on individual members. Implementations are not required
/// to be thread-safe unless explicitly documented by the concrete type.
/// </remarks>
public interface IOrder: ICrud<Order> { }
