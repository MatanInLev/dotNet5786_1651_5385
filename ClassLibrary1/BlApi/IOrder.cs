using BO;

namespace BlApi;

public interface IOrder
{
    BO.Order Get(int userId, int orderId);
    void Update(int userId, BO.Order order);
    void Cancel(int userId, int orderId);
    void Delete(int userId, int orderId);
    void Add(int userId, BO.Order order);
    IEnumerable<BO.OrderInList> GetList(int  userId, BO.OrderStatus? filter, object? filterValue, BO.OrderType? sort);
    Dictionary<BO.OrderStatus, int> GetOrdersStatusCount(int userId);
    void CompleteOrderDelivery(int userId, int deliveryId, BO.DeliveryStatus status);
    public void AssignOrder(int userId, int orderId, int courierId);
    IEnumerable<ClosedDeliveryInList> GetClosedOrdersForCourier(int userId, int courierId, OrderType? typeFilter = null, string? sortProperty = null);
    IEnumerable<OpenOrderInList> GetOpenOrdersForCourier(int userId, int courierId, OrderType? typeFilter = null, string? sortProperty = null);
    
}
