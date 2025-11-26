using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlApi;

public interface ICourier
{
    string Login(string username);
    IEnumerable<CourierInList> GetList(int userId, bool? isActive, Vehicle? vehicle);
    Courier Get(int userId, int courierId);
    void Update(int userId, Courier courier);
    void Delete(int userId, int courierId);
    void Add(int userId, Courier courier);
}
