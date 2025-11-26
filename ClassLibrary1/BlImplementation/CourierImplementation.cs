
namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using System.Collections.Generic;

internal class CourierImplementation : ICourier
{
    public void Add(int userId, Courier courier)
    {
        throw new NotImplementedException();
    }

    public void Delete(int userId, int courierId)
    {
        throw new NotImplementedException();
    }

    public Courier Get(int userId, int courierId)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
