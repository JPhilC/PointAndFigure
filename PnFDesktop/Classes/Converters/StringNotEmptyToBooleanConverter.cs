using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    /// <summary>
    /// A custom boolean to visibility converter that allows a parameter to be passed to
    /// control which Visibility enum value is used with the false boolean.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Boolean))]
    public class StringNotEmptyToBooleanConverter : IValueConverter
    {
      /// <summary>
      /// Returns true if the bound string property is not null or empty.
      /// </summary>
      /// <param name="value"></param>
      /// <param name="targetType"></param>
      /// <param name="parameter"></param>
      /// <param name="culture"></param>
      /// <returns></returns>
      /// <exception cref="ArgumentNullException"></exception>
      /// <exception cref="ArgumentException"></exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? incomingValue = (string?)value;
            return !string.IsNullOrEmpty(incomingValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
