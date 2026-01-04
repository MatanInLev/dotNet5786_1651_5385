using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PL
{
    /// <summary>
    /// Converts a DateTime object to a specific string format (DD/MM/YYYY HH:mm:ss),
    /// ignoring the current culture settings.
    /// </summary>
    public class DateTimeFormatConverter : IValueConverter
    {
        // Converts the DateTime object to the desired string format
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // Force the desired format: Day/Month/Year Hour:Minute:Second
                return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
            }
            return value; // Return original value if not a DateTime
        }

        // Not used for display-only binding
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an OrderStatus to Visibility for the Cancel button.
    /// Only shows Cancel button for Scheduled or InTreatment orders.
    /// </summary>
    public class OrderStatusToCancelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BO.OrderStatus status)
            {
                // Show cancel button only for Scheduled or InTreatment orders
                return (status == BO.OrderStatus.Scheduled || status == BO.OrderStatus.InTreatment)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter that determines courier status based on IsActive and OrderInProgress.
    /// </summary>
    public class CourierStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return "Unknown Status";

            if (values[0] is bool isActive && values[1] is BO.OrderInProgress orderInProgress)
            {
                if (!isActive)
                    return "Not Available";
                
                if (orderInProgress != null)
                    return $"On Delivery (Order #{orderInProgress.OrderId})";
                
                return "Free for Delivery";
            }

            if (values[0] is bool isActiveOnly)
            {
                if (!isActiveOnly)
                    return "Not Available";
                
                return "Free for Delivery";
            }

            return "Unknown Status";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter for courier list status based on IsActive and CurrentOrderId.
    /// </summary>
    public class CourierListStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return "Unknown";

            bool isActive = values[0] is bool active && active;
            int? currentOrderId = values[1] as int?;

            if (!isActive)
                return "Not Available";

            if (currentOrderId.HasValue && currentOrderId.Value > 0)
                return $"On Delivery (#{currentOrderId.Value})";

            return "Free for Delivery";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts OrderStatus to boolean indicating if order can be updated.
    /// Returns true only if order status is Scheduled (Open).
    /// </summary>
    public class OrderCanBeUpdatedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BO.OrderStatus status)
            {
                // Order can only be updated if it's Scheduled (Open)
                return status == BO.OrderStatus.Scheduled;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter to determine if order can be updated or added.
    /// Returns true if it's a new order (Id=0) OR if existing order status is Scheduled.
    /// </summary>
    public class OrderCanBeUpdatedOrIsNewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return true; // Default to enabled

            // values[0] = Order.Id
            // values[1] = Order.Status
            
            if (values[0] is int orderId && values[1] is BO.OrderStatus status)
            {
                // If it's a new order (Id = 0), always enabled
                if (orderId == 0)
                    return true;

                // If it's an existing order, only enabled if status is Scheduled
                return status == BO.OrderStatus.Scheduled;
            }

            return true; // Default to enabled
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}