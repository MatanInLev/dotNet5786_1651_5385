namespace DalApi;

public interface IConfig
{
    DateTime Clock { get; set; }
    int MaxRange { get; set; }
    void Reset();
}
