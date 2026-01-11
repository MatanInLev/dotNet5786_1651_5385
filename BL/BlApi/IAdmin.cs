namespace BlApi;

public interface IAdmin
{
    void ResetDB();
    void InitializeDB();
    DateTime GetClock();
    void ForwardClock(BO.TimeUnit unit);
    BO.Config GetConfig();
    void SetConfig(BO.Config config);

    void StartSimulator(int minutesPerSecond);
    void StopSimulator();

    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
}
