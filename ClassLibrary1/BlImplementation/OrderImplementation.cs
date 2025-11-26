
namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void Add(int userId, Order order)
    {
        throw new NotImplementedException();
    }

    public void Cancel(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void CompleteOrderDelivery(int userId, int deliveryId, DeliveryStatus status)
    {
        throw new NotImplementedException();
    }

    public void Delete(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public Order Get(int userId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OrderInList> GetList(int userId, OrderStatus? filter, object? filterValue, OrderType? sort)
    {
        throw new NotImplementedException();
    }

    public Dictionary<OrderStatus, int> GetOrdersStatusCount(int userId)
    {
        throw new NotImplementedException();
    }

    public void Update(int userId, Order order)
    {
        throw new NotImplementedException();
    }
}
