using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace PnFDesktop.Classes
{
    class AxisLabelTemplateSelector : DataTemplateSelector
    {
        public AxisLabelTemplateSelector()
        {

        }

        public DataTemplate? LeftAxisTemplate
        {
            get;
            set;
        }

        public DataTemplate? RightAxisTemplate
        {
            get;
            set;
        }

        public DataTemplate? BottomAxisTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate? SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is PointAndFigureAxisLabelViewModel vm)
            {
                switch (vm.Location)
                {
                    case AxisLabelLocation.Left:
                        return LeftAxisTemplate;
                        break;
                    case AxisLabelLocation.Right:
                        return RightAxisTemplate;
                        break;
                    case AxisLabelLocation.Bottom:
                        return BottomAxisTemplate;
                        break;
                }
            }


            return base.SelectTemplate(item, container);
        }
    }
}
