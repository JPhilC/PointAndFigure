using PnFData.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PnFDesktop.Classes
{
    [ValueConversion(typeof(PnFColumn), typeof(object))]
    public class ColumnPropertyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PnFColumn column = value as PnFColumn;
            return ColumnPropertyProvider.GetProperty(column, (ColumnPropertyEnum)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
