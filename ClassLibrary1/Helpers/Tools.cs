using System.Collections; 
using System.Reflection;
using System.Text;

namespace BO;

static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "null";

        StringBuilder str = new StringBuilder();
        str.Append("\n");
        str.Append("Type: " + t.GetType().Name + "\n");

        foreach (PropertyInfo item in t.GetType().GetProperties())
        {
            var value = item.GetValue(t, null);

            // התיקון נמצא בשורה הבאה:
            if (value is IEnumerable && !(value is string))
            {
                str.Append(item.Name + ": \n");
                foreach (var listitem in (IEnumerable)value)
                {
                    str.Append("\t" + listitem?.ToString() + "\n");
                }
            }
            else
            {
                str.Append(item.Name + ": " + value + "\n");
            }
        }
        return str.ToString();
    }
}