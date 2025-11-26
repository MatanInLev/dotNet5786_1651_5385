
namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using System;

internal class AdminImplementation : IAdmin
{
    public void ForwardClock(TimeUnit unit)
    {
        switch (unit)
        {
            case TimeUnit.Minutes:
                AdminManager.UpdateClock(AdminManager.Now.AddMinutes(1));
                break;
            case TimeUnit.Hours:
                AdminManager.UpdateClock(AdminManager.Now.AddHours(1));
                break;
            case TimeUnit.Days:
                AdminManager.UpdateClock(AdminManager.Now.AddDays(1));
                break;
            case TimeUnit.months:
                AdminManager.UpdateClock(AdminManager.Now.AddMonths(1));
                break;
            case TimeUnit.years:
                AdminManager.UpdateClock(AdminManager.Now.AddYears(1));
                break;
        }
    }

    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    public Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    public void SetConfig(Config config)
    {
        AdminManager.SetConfig(config);
    }
}
