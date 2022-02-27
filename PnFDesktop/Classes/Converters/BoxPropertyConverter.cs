using PnFData.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    [ValueConversion(typeof(PnFBox), typeof(object))]
    public class BoxPropertyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PnFBox column = value as PnFBox;
            return BoxPropertyProvider.GetProperty(column, (BoxPropertyEnum)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}