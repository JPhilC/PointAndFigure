using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Classes
{
    class PanesStyleSelector : StyleSelector
    {
        public Style ToolStyle
        {
            get;
            set;
        }

        public Style PointAndFigureChartStyle
        {
            get;
            set;
        }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ToolViewModel) {
                return ToolStyle;
            }

            if (item is PointAndFigureChartViewModel)
                return PointAndFigureChartStyle;

            return base.SelectStyle(item, container);
        }
    }
}
