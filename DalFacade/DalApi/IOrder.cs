using DO;
namespace DalApi;

public interface IOrder
{
    void Create(Order item); //Creates new entity object in DAL
    Order? Read(int id); //Reads entity object by its ID 
    List<Order> ReadAll(); //stage 1 only, Reads all entity objects
    void Update(Order item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects
}
