using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Implements an ListBox for displaying PointAndFigureAxisLabels in the NetworkView UI.
    /// </summary>
    public class PointAndFigureAxisLabelItemsControl : ItemsControl
    {
        public PointAndFigureAxisLabelItemsControl()
        {
            //
            // By default, we don't want this UI element to be focusable.
            //
            Focusable = false;
        }

    }
}
