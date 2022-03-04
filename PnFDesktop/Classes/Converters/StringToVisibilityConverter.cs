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
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
      /// <summary>
      /// Returns the visibilty passed as the parameter if the value is NOT an empty string.
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
            
            if (parameter == null) throw new ArgumentNullException("ConverterParameter");
            if (!(parameter is Visibility)) throw new ArgumentException("Visibility required", "ConverterParameter");
            Visibility result = (Visibility)parameter;
            Visibility altResult = Visibility.Collapsed;
            if (result == Visibility.Collapsed)
            {
                altResult = Visibility.Visible;
            }
            string? incomingValue = (string?)value;
            if (!string.IsNullOrEmpty(incomingValue))
            {
                return result;
            }
            else
            {
                return altResult;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
