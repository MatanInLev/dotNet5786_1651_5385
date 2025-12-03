namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void Add(int userId, Order order)
    {
        OrderManager.Create(order);
    }

    public void Cancel(int userId, int orderId)
    {
        OrderManager.CancelOrder(orderId);
    }

    public void CompleteOrderDelivery(int userId, int deliveryId, DeliveryStatus status)
    {
        OrderManager.CompleteOrder(deliveryId, status);
    }

    public void Delete(int userId, int orderId)
    {
        OrderManager.Delete(orderId);
    }

    public Order Get(int userId, int orderId)
    {
        return OrderManager.Get(orderId);
    }

    public IEnumerable<OrderInList> GetList(int userId, OrderStatus? filter, object? filterValue, OrderType? sort)
    {
        return OrderManager.GetList(filter);
    }

    public Dictionary<OrderStatus, int> GetOrdersStatusCount(int userId)
    {
        return OrderManager.GetOrdersStatusCount();
    }

    public void Update(int userId, Order order)
    {
        OrderManager.Update(order);
    }
    public void AssignOrder(int userId, int orderId, int courierId)
    {
        OrderManager.AssignOrderToCourier(orderId, courierId);
    }
    public IEnumerable<ClosedDeliveryInList> GetClosedOrdersForCourier(int userId, int courierId, OrderType? typeFilter = null, string? sortProperty = null)
    {
        return OrderManager.GetClosedOrdersForCourier(courierId, typeFilter, sortProperty);
    }

    public IEnumerable<OpenOrderInList> GetOpenOrdersForCourier(int userId, int courierId, OrderType? typeFilter = null, string? sortProperty = null)
    {
        return OrderManager.GetOpenOrdersForCourier(courierId, typeFilter, sortProperty);
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
    OrderManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    OrderManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    OrderManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    OrderManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
}
