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

        internal static int CompanyAddress
        {
            get => XMLTools.GetConfigStrVal(config_file_name, "CompanyAddress");
            set => XMLTools.SetConfigStrVal(config_file_name, "CompanyAddress", value);
        }
    }
}
