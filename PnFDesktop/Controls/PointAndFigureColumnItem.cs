using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// This is a UI element that represents a network/flow-chart PointAndFigureColumn.
    /// </summary>
    public class PointAndFigureColumnItem : ListBoxItem
    {
        #region Dependency Property/Event Definitions

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BoxOffsetXProperty =
           DependencyProperty.Register("BoxOffsetX", typeof(double), typeof(PointAndFigureColumnItem),
              new FrameworkPropertyMetadata(0.0));
        public static readonly DependencyProperty BoxOffsetYProperty =
           DependencyProperty.Register("BoxOffsetY", typeof(double), typeof(PointAndFigureColumnItem),
              new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ParentPointAndFigureChartProperty =
            DependencyProperty.Register("ParentPointAndFigureChart", typeof(PointAndFigureChart), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(ParentPointAndFigureChart_PropertyChanged));


        #endregion Dependency Property/Event Definitions

        public PointAndFigureColumnItem()
        {
            //
            // By default, we don't want this UI element to be focusable.
            //
            Focusable = false;
        }

        /// <summary>
        /// The X coordinate of the PointAndFigureColumn.
        /// </summary>
        public double X
        {
            get => (double)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary>
        /// The Y coordinate of the PointAndFigureColumn.
        /// </summary>
        public double Y
        {
            get => (double)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        /// <summary>
        /// The X offset for the box.
        /// </summary>
        public double BoxOffsetX
        {
            get => (double)GetValue(BoxOffsetXProperty);
            set => SetValue(BoxOffsetXProperty, value);
        }

        /// <summary>
        /// The Y offset or the box.
        /// </summary>
        public double BoxOffsetY
        {
            get => (double)GetValue(BoxOffsetYProperty);
            set => SetValue(BoxOffsetYProperty, value);
        }

        /// <summary>
        /// The Z index of the PointAndFigureColumn.
        /// </summary>
        public int ZIndex
        {
            get => (int)GetValue(ZIndexProperty);
            set => SetValue(ZIndexProperty, value);
        }

        public void Select()
        {
            //
            // Clear the selection and select the clicked item as the only selected item.
            //
            this.ParentPointAndFigureChart.SelectedColumn = null;
            this.IsSelected = true;
        }


        #region Private Data Members\Properties

        internal PointAndFigureBoxItem GetPointAndFigureBox()
        {
            // Find the PointAndFigureColumn items ItemsControl in the visual tree
            ItemsControl connectorsControl = FindVisualChild<ItemsControl>(this);
            // Get the ContentPresenter for the first connector
            ContentPresenter connectorCp = (ContentPresenter)connectorsControl.ItemContainerGenerator.ContainerFromItem(connectorsControl.Items[0]);
            // Get the PointAndFigureBoxItem from the template
            PointAndFigureBoxItem connector = connectorCp.ContentTemplate.FindName("PointAndFigureBoxItem", connectorCp) as PointAndFigureBoxItem;
            if (connector == null)
            {
                throw new ApplicationException("Template part 'ConnectionItem' is not found on PointAndFigureBoxItem template");
            }
            return connector;
        }

        private TChildItem FindVisualChild<TChildItem>(DependencyObject obj)
             where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is TChildItem)
                {
                    return (TChildItem)child;
                }
                else
                {
                    TChildItem childOfChild = FindVisualChild<TChildItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Reference to the data-bound parent PointAndFigureChart.
        /// </summary>
        public PointAndFigureChart ParentPointAndFigureChart
        {
            get => (PointAndFigureChart)GetValue(ParentPointAndFigureChartProperty);
            set => SetValue(ParentPointAndFigureChartProperty, value);
        }

        /// <summary>
        /// Set to 'true' when left mouse button is held down.
        /// </summary>
        private bool _isLeftMouseDown;

        /// <summary>
        /// Set to 'true' when left mouse button and the control key are held down.
        /// </summary>
        private bool _isLeftMouseAndControlDown;



        #endregion Private Data Members\Properties

        #region Private Methods

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PointAndFigureColumnItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointAndFigureColumnItem), new FrameworkPropertyMetadata(typeof(PointAndFigureColumnItem)));
        }

        /// <summary>
        /// Bring the PointAndFigureColumn to the front of other elements.
        /// </summary>
        internal void BringToFront()
        {
            if (this.ParentPointAndFigureChart == null)
            {
                return;
            }

            int maxZ = this.ParentPointAndFigureChart.FindMaxZIndex();
            this.ZIndex = maxZ + 1;
        }

        /// <summary>
        /// Called when a mouse button is held down.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BringToFront();

            if (this.ParentPointAndFigureChart != null)
            {
                this.ParentPointAndFigureChart.Focus();
            }

            if (e.ChangedButton == MouseButton.Left && this.ParentPointAndFigureChart != null)
            {
                _isLeftMouseDown = true;

                LeftMouseDownSelectionLogic();

                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right && this.ParentPointAndFigureChart != null)
            {
                RightMouseDownSelectionLogic();
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that PointAndFigureBoxItem can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void LeftMouseDownSelectionLogic()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                //
                // Control key was held down.
                // This means that the rectangle is being added to or removed from the existing selection.
                // Don't do anything yet, we will act on this later in the MouseUp event handler.
                //
                _isLeftMouseAndControlDown = true;
            }
            else
            {
                //
                // Control key is not held down.
                //
                _isLeftMouseAndControlDown = false;

                if (this.ParentPointAndFigureChart.SelectedColumn == null)
                {
                    //
                    // Nothing already selected, select the item.
                    //
                    this.IsSelected = true;
                }
                else if (this.ParentPointAndFigureChart.SelectedColumn.Equals(this) ||
                         this.ParentPointAndFigureChart.SelectedColumn.Equals(this.DataContext))
                {
                    // 
                    // Item is already selected, do nothing.
                    // We will act on this in the MouseUp if there was no drag operation.
                    //
                }
                else
                {
                    //
                    // Item is not selected.
                    // Deselect all, and select the item.
                    //
                    this.ParentPointAndFigureChart.SelectedColumn = null;
                    this.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the right mouse button is pressed down.
        /// The reason this exists in its own method rather than being included in OnMouseDown is 
        /// so that PointAndFigureBoxItem can reuse this logic from its OnMouseDown.
        /// </summary>
        internal void RightMouseDownSelectionLogic()
        {
            if (this.ParentPointAndFigureChart.SelectedColumn == null)
            {
                //
                // Nothing already selected, select the item.
                //
                this.IsSelected = true;
            }
            else if (this.ParentPointAndFigureChart.SelectedColumn.Equals(this) ||
                     this.ParentPointAndFigureChart.SelectedColumn.Equals(this.DataContext))
            {
                // 
                // Item is already selected, do nothing.
                //
            }
            else
            {
                //
                // Item is not selected.
                // Deselect all, and select the item.
                //
                this.ParentPointAndFigureChart.SelectedColumn = null;
                this.IsSelected = true;
            }
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isLeftMouseDown)
            {
                //
                // Execute mouse up selection logic only if there was no drag operation.
                //

                LeftMouseUpSelectionLogic();

                _isLeftMouseDown = false;
                _isLeftMouseAndControlDown = false;

                e.Handled = true;
            }
        }

        /// <summary>
        /// This method contains selection logic that is invoked when the left mouse button is released.
        /// The reason this exists in its own method rather than being included in OnMouseUp is 
        /// so that PointAndFigureBoxItem can reuse this logic from its OnMouseUp.
        /// </summary>
        internal void LeftMouseUpSelectionLogic()
        {
            if (_isLeftMouseAndControlDown)
            {
                //
                // Control key was held down.
                // Toggle the selection.
                //
                this.IsSelected = !this.IsSelected;
            }
            else
            {
                //
                // Control key was not held down.
                //
                if (this.ParentPointAndFigureChart.SelectedColumn.Equals(this) ||
                     this.ParentPointAndFigureChart.SelectedColumn.Equals(this.DataContext))
                {
                    //
                    // The item that was clicked is already the only selected item.
                    // Don't need to do anything.
                    //
                }
                else
                {
                    //
                    // Clear the selection and select the clicked item as the only selected item.
                    //
                    this.ParentPointAndFigureChart.SelectedColumn = null;
                    this.IsSelected = true;
                }
            }

            _isLeftMouseAndControlDown = false;
        }


        /// <summary>
        /// Event raised when the ParentPointAndFigureChart property has changed.
        /// </summary>
        private static void ParentPointAndFigureChart_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //
            // Bring new PointAndFigureColumns to the front of the z-order.
            //
            var pointAndFigureColumnItem = (PointAndFigureColumnItem)o;
            pointAndFigureColumnItem.BringToFront();
        }

        #endregion Private Methods
    }
}
