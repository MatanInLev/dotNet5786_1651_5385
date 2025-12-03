namespace BlApi;

public interface ICourier : IObservable
{
    string Login(string username);
    IEnumerable<BO.CourierInList> GetList(int userId, bool? isActive = null, BO.Vehicle? vehicle = null);
    BO.Courier Get(int userId, int courierId);
    void Update(int userId, BO.Courier courier);
    void Delete(int userId, int courierId);
    void Add(int userId, BO.Courier courier);
}
