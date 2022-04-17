using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace PnFDesktop.Classes
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }

        public DataTemplate? PointAndFigureChartViewTemplate
        {
            get;
            set;
        }
        public DataTemplate? MarketSummaryViewTemplate
        {
            get;
            set;
        }

        public DataTemplate? MessagesViewTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is PointAndFigureChartViewModel)
            {
                return PointAndFigureChartViewTemplate!;
            }

            if (item is MarketSummaryViewModel)
            {
                return MarketSummaryViewTemplate!;
            }

            if (item is MessagesViewModel)
                return MessagesViewTemplate!;

            return base.SelectTemplate(item, container);
        }
    }
}
