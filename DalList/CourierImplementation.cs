namespace Dal;
using DalApi;
using DalList;
using DO;
using System.Collections.Generic;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if (DataSource.Couriers.Find(item => item.Id == item.Id)!=null)
        {
            throw new InvalidOperationException("An object of type courier with ID: {id} already exist.");
        }
        DataSource.Couriers.Add(item);

    }
   
    public void Delete(int id)
    {
        int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == id);
        if (index == -1)
            throw new InvalidOperationException("An object of type courier with ID: {id} does not exist.");
        DataSource.Couriers.RemoveAt(index);
    }

    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    public Courier? Read(int id)
    {
        Courier? orderToFind = DataSource.Couriers.Find(Courier => Courier.Id == id);
        return orderToFind;
    }

    public List<Courier> ReadAll()
    {
        return new List<Courier>(DataSource.Couriers);
    }

    public void Update(Courier item)
    {
        int index = DataSource.Couriers.FindIndex(Courier => Courier.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException("An object of type courier with ID: {item.Id} does not exist.");
        DataSource.Couriers[index] = item;
    }
}
