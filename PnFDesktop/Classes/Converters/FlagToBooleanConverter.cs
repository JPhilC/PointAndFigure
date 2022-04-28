using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    /// <summary>
    /// A custom flag to boolean convert that requires.
    /// </summary>
    [ValueConversion(typeof(System.Enum), typeof(Boolean))]
    public class FlagToBooleanConverter : IValueConverter
    {
      /// <summary>
      /// Returns true or false depending on whether the enum flag
      /// matches the parameter passed.
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
            if (!(parameter is System.Enum)) throw new ArgumentException("Enum value required required", "ConverterParameter");
            int boundValue = (int)value;
            int comparator = (int)parameter;
            return (boundValue&comparator) == comparator;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
