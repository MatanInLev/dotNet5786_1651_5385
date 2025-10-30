using DalList;

namespace Dal;

public class ConfigImplementation
{
    public DateTime Clock
    {
        get => Config.Instance.Clock;
        set => Config.Instance.Clock = value;
    }
    public int MaxRange
    {
        get => Config.Instance.MaxRange;
        set => Config.Instance.MaxRange = value;
    }
    //...
    public void Reset()
    {
        Config.Instance.Reset();
    }


}
