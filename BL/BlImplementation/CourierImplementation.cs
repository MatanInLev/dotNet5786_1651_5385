
namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using System.Collections.Generic;

internal class CourierImplementation : ICourier
{
    public void Add(int userId, Courier courier)
    {
        CourierManager.Create(courier);
    }

    public void Delete(int userId, int courierId)
    {
        CourierManager.Delete(courierId);
    }

    public Courier Get(int userId, int courierId)
    {
        return CourierManager.Get(courierId);
    }

    public IEnumerable<CourierInList> GetList(int userId, bool? isActive = null, Vehicle? vehicle = null)
    {
        return CourierManager.GetCouriersList(userId, isActive, vehicle);
    }

    public string Login(string userId)
    {
        return CourierManager.Login(userId);
    }

    public void Update(int userId, Courier courier)
    {
        CourierManager.Update(courier);
    }
}
