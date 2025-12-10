
using System;
using System.Globalization;
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
}