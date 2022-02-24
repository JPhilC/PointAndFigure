using System;
using System.Globalization;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleNaNConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(double.NaN) ? string.Empty : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sValue = (string)value;
            if (string.IsNullOrWhiteSpace(sValue))
            {
                return double.NaN;
            }
            else
            {
                double dValue = 0.0;
                if (double.TryParse(sValue, out dValue))
                {
                    return dValue;
                }
                else
                {
                    return double.NaN;
                }

            }
        }
    }
}
