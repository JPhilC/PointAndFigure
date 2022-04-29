using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Classes
{
    class PanesStyleSelector : StyleSelector
    {
        public Style? ToolStyle
        {
            get;
            set;
        }

        public Style? DocumentStyle
        {
            get;
            set;
        }

        public override System.Windows.Style? SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is ToolViewModel) {
                return ToolStyle;
            }

            if (item is PointAndFigureChartViewModel)
                return DocumentStyle;

            if (item is MarketSummaryViewModel)
                return DocumentStyle;

            if (item is FilteredSharesSummaryViewModel)
                return DocumentStyle;

            if (item is SharesSummaryViewModel)
                return DocumentStyle;

            return base.SelectStyle(item, container);
        }
    }
}
