using DalApi;
using DalList;
using DO;
using System;

namespace Dal;

public class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        int newId = Config.NextOrderId;
        Order orderToAdd = item with { Id = newId };
        DataSource.Orders.Add(orderToAdd);
    }

    public void Delete(int id)
    {
        int index =DataSource.Orders.FindIndex(order=>order.Id == id);
        if (index == -1)
            throw new InvalidOperationException("An object of type Order with such ID does not exist.");
        DataSource.Orders.RemoveAt(index);
    }

    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    public Order? Read(int id)
    {
       Order? orderToFind = DataSource.Orders.Find(order=>order.Id == id);
       return orderToFind;
    }

    public List<Order> ReadAll()
    {
        return new List<Order>(DataSource.Orders);
    }   

    public void Update(Order item)
    {
        int index = DataSource.Orders.FindIndex(order => order.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException("An object of type Order with such ID does not exist.");
        DataSource.Orders[index] = item;
    }
}
