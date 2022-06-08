using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Implements an ListBox for displaying PointAndFigureHighlight in the NetworkView UI.
    /// </summary>
    public class PointAndFigureHighlightItemsControl : ItemsControl
    {
        public PointAndFigureHighlightItemsControl()
        {
            //
            // By default, we don't want this UI element to be focusable.
            //
            Focusable = false;
        }
    }
}
