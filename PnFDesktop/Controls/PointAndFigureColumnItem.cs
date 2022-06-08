using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PnFDesktop.Classes;
using PnFDesktop.ViewModels;

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
        public static readonly DependencyProperty BullishSupportYProperty =
            DependencyProperty.Register("BullishSupportY", typeof(double), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private static readonly DependencyPropertyKey BoxesPropertyKey =
                 DependencyProperty.RegisterReadOnly("Boxes", typeof(ImpObservableCollection<object>), typeof(PointAndFigureColumnItem),
                        new FrameworkPropertyMetadata());

        public static readonly DependencyProperty BoxesProperty = BoxesPropertyKey.DependencyProperty;

        public static readonly DependencyProperty BoxesSourceProperty =
            DependencyProperty.Register("BoxesSource", typeof(IEnumerable), typeof(PointAndFigureColumnItem),
                new FrameworkPropertyMetadata(BoxesSource_PropertyChanged));

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'BoxesSource' property.
        /// </summary>
        private static void BoxesSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointAndFigureColumnItem c = (PointAndFigureColumnItem)d;

            //
            // Clear 'Boxes'.
            //
            c.Boxes.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(c.BoxesSource_CollectionChanged);
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    //
                    // Populate 'Boxes' from 'BoxesSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.Boxes.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(c.BoxesSource_CollectionChanged);
                }
            }
        }

        /// <summary>
        /// Event raised when a column has been added to or removed from the collection assigned to 'BoxesSource'.
        /// </summary>
        private void BoxesSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'BoxesSource' has been cleared, also clear 'Boxes'.
                //
                Boxes.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'BoxesSource' also remove it from 'Boxes'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        Boxes.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'BoxesSource' also add it to 'Boxes'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        Boxes.Add(obj);
                    }
                }
            }
        }

        #endregion Dependency Property/Event Definitions


        public PointAndFigureColumnItem()
        {
            //
            // By default, we don't want this UI element to be focusable.
            //
            Focusable = false;
            SetValue(BoxesPropertyKey, new ImpObservableCollection<object>());
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PointAndFigureColumnItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointAndFigureColumnItem), new FrameworkPropertyMetadata(typeof(PointAndFigureColumnItem)));
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
        /// The Y coordinate of the Bullish Support box.
        /// </summary>
        public double BullishSupportY
        {
            get => (double)GetValue(BullishSupportYProperty);
            set => SetValue(BullishSupportYProperty, value);
        }

                /// <summary>
        /// Collection of Columns in the chart.
        /// </summary>
        public ImpObservableCollection<object> Boxes => (ImpObservableCollection<object>)GetValue(BoxesProperty);

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Columns'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable BoxesSource
        {
            get => (IEnumerable)GetValue(BoxesSourceProperty);
            set => SetValue(BoxesSourceProperty, value);
        }

    }
}
