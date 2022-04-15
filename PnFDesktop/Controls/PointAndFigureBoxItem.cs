using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// This is the UI element for a connector.
    /// Each nodes has multiple connectors that are used to connect it to other nodes.
    /// </summary>
    public class PointAndFigureBoxItem : ContentControl
    {
        #region Dependency Property/Event Definitions
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(PointAndFigureBoxItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(PointAndFigureBoxItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(PointAndFigureBoxItem),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(PointAndFigureBoxItem),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty ParentPointAndFigureChartProperty =
            DependencyProperty.Register("ParentPointAndFigureChart", typeof(PointAndFigureChartControl), typeof(PointAndFigureBoxItem),
                new FrameworkPropertyMetadata(ParentPointAndFigureChart_PropertyChanged));

        public static readonly DependencyProperty ParentPointAndFigureColumnItemProperty =
            DependencyProperty.Register("ParentPointAndFigureColumnItem", typeof(PointAndFigureColumnItem), typeof(PointAndFigureBoxItem));


        #endregion Dependency Property/Event Definitions


        /// <summary>
        /// Reference to the data-bound parent PointAndFigureChart.
        /// </summary>
        public PointAndFigureChartControl ParentPointAndFigureChart
        {
            get => (PointAndFigureChartControl)GetValue(ParentPointAndFigureChartProperty);
            set => SetValue(ParentPointAndFigureChartProperty, value);
        }


        /// <summary>
        /// Reference to the data-bound parent PointAndFigureColumnItem.
        /// </summary>
        public PointAndFigureColumnItem ParentPointAndFigureColumnItem
        {
            get => (PointAndFigureColumnItem)GetValue(ParentPointAndFigureColumnItemProperty);
            set => SetValue(ParentPointAndFigureColumnItemProperty, value);
        }

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

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        public int ZIndex
        {
            get => (int)GetValue(ZIndexProperty);
            set => SetValue(ZIndexProperty, value);
        }

        /// <summary>
        /// The Image used for the connector.
        /// </summary>
        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }


        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PointAndFigureBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointAndFigureBoxItem), new FrameworkPropertyMetadata(typeof(PointAndFigureBoxItem)));
        }


        /// <summary>
        /// Event raised when 'ParentPointAndFigureChart' property has changed.
        /// </summary>
        private static void ParentPointAndFigureChart_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion Private Methods
    }
}
