using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace PnFDesktop.Classes
{
    class HighlightTemplateSelector : DataTemplateSelector
    {
        public HighlightTemplateSelector()
        {

        }

        public DataTemplate? HorizontalTemplate
        {
            get;
            set;
        }

        public DataTemplate? VerticalTemplate
        {
            get;
            set;
        }


        public override System.Windows.DataTemplate? SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is PointAndFigureHighlightViewModel vm)
            {
                switch (vm.Orientation)
                {
                    case Orientation.Horizontal:
                        return HorizontalTemplate;
                        break;
                    case Orientation.Vertical:
                        return VerticalTemplate;
                        break;
                }
            }


            return base.SelectTemplate(item, container);
        }
    }
}
