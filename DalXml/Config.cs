using System;
using System.Xml.Linq;


namespace Dal
{
    internal static class Config
    {
        internal const string config_file_name = "data-config.xml";
        internal const string courier_file_name = "couriers.xml";
        internal const string delivery_file_name = "deliveries.xml";
        internal const string order_file_name = "orders.xml";

        internal static int NextOrderId {
            get => XMLTools.GetAndIncreaseConfigIntVal(config_file_name, "NextOrderId");
            private set => XMLTools.GetConfigIntVal(config_file_name, "NextOrderId");
        }
        internal static int NextCourierId
        {
            get => XMLTools.GetAndIncreaseConfigIntVal(config_file_name, "NextCourierId");
            private set => XMLTools.GetConfigIntVal(config_file_name, "NextCourierId");
        }
        internal static int NextDeliveryId
        {
            get => XMLTools.GetAndIncreaseConfigIntVal(config_file_name, "NextDeliveryId");
            private set => XMLTools.GetConfigIntVal(config_file_name, "NextDeliveryId");
        }
        internal static DateTime Clock
        {
            get => XMLTools.GetConfigDateVal(config_file_name, "Clock");
            set => XMLTools.SetConfigDateVal(config_file_name, "Clock", value);
        }
        internal static int MaxRange
        {
            get => XMLTools.GetConfigIntVal(config_file_name, "MaxRange");
            set => XMLTools.SetConfigIntVal(config_file_name, "MaxRange", value);
        }
        internal static int AdminId
        {
            get => XMLTools.GetConfigIntVal(config_file_name, "AdminId");
            set => XMLTools.SetConfigIntVal(config_file_name, "AdminId", value);
        }
        internal static string CompanyAddress
        {
            get => XMLTools.GetConfigStrVal(config_file_name, "CompanyAddress");
            set => XMLTools.SetConfigStrVal(config_file_name, "CompanyAddress", value);
        }
        internal static double Latitude
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "Latitude");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "Latitude", value);
        }
        internal static double Longitude
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "Longitude");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "Longitude", value);
        }
        internal static double MaxGeneralDistance
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "MaxGeneralDistance");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "MaxGeneralDistance", value);
        }
        internal static double AvgCarSpeed
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "AvgCarSpeed");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "AvgCarSpeed", value);
        }
        internal static double AvgMotorcycleSpeed
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "AvgMotorcycleSpeed");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "AvgMotorcycleSpeed", value);
        }
        internal static double AvgBicycleSpeed
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "AvgBicycleSpeed");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "AvgBicycleSpeed", value);
        }
        internal static double AvgFootSpeed
        {
            get => XMLTools.GetConfigIDoubleVal(config_file_name, "AvgFootSpeed");
            set => XMLTools.SetConfigDoubleVal(config_file_name, "AvgFootSpeed", value);
        }
        internal static TimeSpan MaxDeliveryTime
        {
            get => XMLTools.GetConfigTimeSpanVal(config_file_name, "MaxDeliveryTime");
            set => XMLTools.SetConfigTimeSpanVal(config_file_name, "MaxDeliveryTime", value);
        }
        internal static TimeSpan RiskRange
        {
            get => XMLTools.GetConfigTimeSpanVal(config_file_name, "RiskRange");
            set => XMLTools.SetConfigTimeSpanVal(config_file_name, "RiskRange", value);
        }
        internal static TimeSpan InactivityRange
        {
            get => XMLTools.GetConfigTimeSpanVal(config_file_name, "InactivityRange");
            set => XMLTools.SetConfigTimeSpanVal(config_file_name, "InactivityRange", value);
        }
        internal static void Reset()
        {
            NextOrderId = 0;
            NextDeliveryId = 0;
            NextCourierId = 0;
            Clock = DateTime.Now;
            MaxRange = 0;
            AdminId = 12345678;
            CompanyAddress = "";
            Latitude = 31.765847796216843;
            Longitude = 35.19104519946595;
            MaxGeneralDistance = 0;
            AvgCarSpeed = 60;
            AvgMotorcycleSpeed = 70;
            AvgBicycleSpeed = 20;
            AvgFootSpeed = 5;
            MaxDeliveryTime = TimeSpan.FromHours(2);
            RiskRange = TimeSpan.FromMinutes(15);
            InactivityRange = TimeSpan.FromDays(3);
        }
    }
}
