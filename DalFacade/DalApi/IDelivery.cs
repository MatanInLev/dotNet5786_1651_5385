// Module IDelivery.cs
using DO;
namespace DalApi;

public interface IDelivery
{
    void Create(Delivery item); //Creates new entity object in DAL
    Delivery? Read(int id); //Reads entity object by its ID 
    List<Delivery> ReadAll(); //stage 1 only, Reads all entity objects
    void Update(Delivery item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects
}
