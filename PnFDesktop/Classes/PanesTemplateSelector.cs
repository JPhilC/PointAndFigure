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

        public DataTemplate? DocumentViewTemplate
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
                return DocumentViewTemplate!;
            }

            if (item is MarketSummaryViewModel)
            {
                return DocumentViewTemplate!;
            }

            if (item is SharesSummaryViewModel)
            {
                return DocumentViewTemplate!;
            }

            if (item is FilteredSharesSummaryViewModel)
            {
                return DocumentViewTemplate!;
            }

            if (item is MessagesViewModel)
                return MessagesViewTemplate!;

            return base.SelectTemplate(item, container);
        }
    }
}
