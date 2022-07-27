using System;
using System.Windows.Data;
using System.Globalization;

namespace PnFDesktop.Classes
{
    /// <summary>
    /// Used to round a double value to a number of decimals
    /// </summary>
    public class DoubleRoundingConverter : IValueConverter
    {
        /// <summary>
        /// Convert a fraction to a percentage.
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int decimals = 0;
            if (parameter != null)
            {
                decimals = (int)parameter; 
            }
            if (value == null)
            {
                return 0d;
            }
            return (double)Math.Round((double)value, decimals);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
