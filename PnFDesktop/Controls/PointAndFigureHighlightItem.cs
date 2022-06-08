using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// This is the UI element for a connector.
    /// Each nodes has multiple connectors that are used to connect it to other nodes.
    /// </summary>
    public class PointAndFigureHighlightItem : ContentControl
    {
        #region Dependency Property/Event Definitions
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(PointAndFigureHighlightItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(PointAndFigureHighlightItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion Dependency Property/Event Definitions



        /// <summary>
        /// The X coordinate of the connector.
        /// </summary>
        public double X
        {
            get => (double)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary>
        /// The Y coordinate of the connector.
        /// </summary>
        public double Y
        {
            get => (double)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PointAndFigureHighlightItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointAndFigureHighlightItem), new FrameworkPropertyMetadata(typeof(PointAndFigureHighlightItem)));
        }

        #endregion Private Methods
    }
}
