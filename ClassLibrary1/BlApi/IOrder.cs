using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;
namespace BlApi;

public interface IOrder
{
    Order Get(int userId, int orderId);
    void Update(int userId, Order order);
    void Cancel(int userId, int orderId);
    void Delete(int userId, int orderId);
    void Add(int userId, Order order);
    IEnumerable<OrderInList> GetList(int  userId, OrderStatus? filter, object? filterValue, OrderType? sort);
    Dictionary<OrderStatus, int> GetOrdersStatusCount(int userId);
    void CompleteOrderDelivery(int userId, int deliveryId, DeliveryStatus status);
}
