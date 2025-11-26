namespace BO;
public class Config
{
    public int MaxRange { get; set; }
    public string? CompanyAddress { get; set; }
    public double CarSpeed { get; set; }
    public double MotorcycleSpeed { get; set; }
    public double BicycleSpeed { get; set; }
    public double FootSpeed { get; set; }
    public TimeSpan MaxDeliveryTime { get; set; }
    public TimeSpan RiskRange { get; set; }
    public TimeSpan InactivityTime { get; set; }
}
