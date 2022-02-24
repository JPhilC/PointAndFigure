using System;
using System.Globalization;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    [ValueConversion(typeof(Boolean), typeof(Boolean))]
    public class BooleanInverterConverter : IValueConverter
    {
        /// <summary>
        /// Converts a Boolean value to the opposite boolean value
        /// </summary>
        /// <param name="value">The underlying property value</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">This parameter is not used.</param>
        /// <returns> Boolean</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// Converts a Boolean value to the opposite boolean value
        /// </summary>
        /// <param name="value">The displayed value</param>
        /// <param name="targetType">This parameter is not used.</param>
        /// <param name="parameter">This parameter is not used.</param>
        /// <param name="culture">true if value is System.Windows.Visibility.Collapsed or Hidden; otherwise, false.</param>
        /// <returns>Boolean</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

    }
}
