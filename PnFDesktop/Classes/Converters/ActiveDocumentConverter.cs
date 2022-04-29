using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PnFDesktop.Interfaces;
using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;

namespace PnFDesktop.Classes
{
    class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IPointAndFigureChartViewModel 
                || value is MarketSummaryViewModel
                || value is FilteredSharesSummaryViewModel)
            {
                return value;
            }


            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is IPointAndFigureChartViewModel
                || value is MarketSummaryViewModel
                || value is FilteredSharesSummaryViewModel)
            {
                return value;
            }

            return Binding.DoNothing;
        }
    }
}
