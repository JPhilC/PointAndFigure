using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;

namespace PnFDesktop.Classes
{
    class PanesStyleSelector : StyleSelector
    {
        public Style PointAndFigureChartStyle
        {
            get;
            set;
        }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is PointAndFigureChartViewModel)
                return PointAndFigureChartStyle;

            return base.SelectStyle(item, container);
        }
    }
}
