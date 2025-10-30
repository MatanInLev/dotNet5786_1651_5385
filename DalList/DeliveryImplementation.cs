using DalApi;
using DalList;
using DO;

namespace Dal;

public class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        int newId = Config.NextDeliveryId;
        Delivery deliveryToAdd = item with { Id = newId };
        DataSource.Deliveries.Add(deliveryToAdd);
    }

    public void Delete(int id)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == id);
        if (index == -1)
            throw new InvalidOperationException("An object of type Delivery with ID: {id} does not exist.");
        DataSource.Deliveries.RemoveAt(index);
    }

    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    public Delivery? Read(int id)
    {
        Delivery? deliveryToFind = DataSource.Deliveries.Find(delivery => delivery.Id == id);
        return deliveryToFind;
    }

    public List<Delivery> ReadAll()
    {
        return new List<Delivery>(DataSource.Deliveries);
    }

    public void Update(Delivery item)
    {
        int index = DataSource.Deliveries.FindIndex(delivery => delivery.Id == item.Id);
        if (index == -1)
            throw new InvalidOperationException("An object of type Delivery with ID: {item.Id} does not exist.");
        DataSource.Deliveries[index] = item;
    }
}
