﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Windows.Input;
using PnFDesktop.Classes;


namespace PnFDesktop.Controls
{
    public class PointAndFigureChartControl : Control
    {
        #region Dependency Property/Event Definitions

        #region Columns ...
        private static readonly DependencyPropertyKey ColumnsPropertyKey =
            DependencyProperty.RegisterReadOnly("Columns", typeof(ImpObservableCollection<object>), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty ColumnsProperty = ColumnsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.Register("ColumnsSource", typeof(IEnumerable), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(ColumnsSource_PropertyChanged));

        public static readonly DependencyProperty SelectedColumnProperty =
            DependencyProperty.Register("SelectedColumn", typeof(object), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedColumnChanged), new CoerceValueCallback(CoerceSelectedColumnValue)));

        public static readonly DependencyProperty ColumnItemTemplateProperty =
            DependencyProperty.Register("ColumnItemTemplate", typeof(DataTemplate), typeof(PointAndFigureChartControl));

        public static readonly DependencyProperty ColumnItemTemplateSelectorProperty =
            DependencyProperty.Register("ColumnItemTemplateSelector", typeof(DataTemplateSelector), typeof(PointAndFigureChartControl));

        public static readonly DependencyProperty ColumnItemContainerStyleProperty =
            DependencyProperty.Register("ColumnItemContainerStyle", typeof(Style), typeof(PointAndFigureChartControl));

        public static readonly DependencyProperty IsClearSelectionOnEmptySpaceClickEnabledProperty =
            DependencyProperty.Register("IsClearSelectionOnEmptySpaceClickEnabled", typeof(bool), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(true));
        #endregion


        #region row and column highlights ...
        private static readonly DependencyPropertyKey HighlightsPropertyKey =
            DependencyProperty.RegisterReadOnly("Highlights", typeof(ImpObservableCollection<object>), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty HighlightsProperty = HighlightsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty HighlightsSourceProperty =
            DependencyProperty.Register("HighlightsSource", typeof(IEnumerable), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(HighlightsSource_PropertyChanged));

        public static readonly DependencyProperty HighlightItemTemplateSelectorProperty =
            DependencyProperty.Register("HighlightItemTemplateSelector", typeof(DataTemplateSelector), typeof(PointAndFigureChartControl));
        #endregion

        #region axis labels ...
        private static readonly DependencyPropertyKey AxisLabelsPropertyKey =
            DependencyProperty.RegisterReadOnly("AxisLabels", typeof(ImpObservableCollection<object>), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty AxisLabelsProperty = AxisLabelsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty AxisLabelsSourceProperty =
            DependencyProperty.Register("AxisLabelsSource", typeof(IEnumerable), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(AxisLabelsSource_PropertyChanged));

        public static readonly DependencyProperty AxisLabelItemTemplateSelectorProperty =
            DependencyProperty.Register("AxisLabelItemTemplateSelector", typeof(DataTemplateSelector), typeof(PointAndFigureChartControl));
        #endregion


        private static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register("GridSize", typeof(double), typeof(PointAndFigureChartControl),
                new FrameworkPropertyMetadata(5d));

        #endregion

        #region Private Data Members

        /// <summary>
        /// Cached reference to the ColumnItemsControl in the visual-tree.
        /// </summary>
        private PointAndFigureColumnItemsControl _columnItemsControl;

        /// <summary>
        /// Cached the currently selected column.
        /// </summary>
        private List<object> _initialSelectedColumns;


        #endregion

        public PointAndFigureChartControl()
        {
            SetValue(HighlightsPropertyKey, new ImpObservableCollection<object>());
            SetValue(AxisLabelsPropertyKey, new ImpObservableCollection<object>());
            SetValue(ColumnsPropertyKey, new ImpObservableCollection<object>());
        }


        /// <summary>
        /// Static constructor.
        /// </summary>
        static PointAndFigureChartControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PointAndFigureChartControl), new FrameworkPropertyMetadata(typeof(PointAndFigureChartControl)));
        }


        /// <summary>
        /// Collection of Columns in the chart.
        /// </summary>
        public ImpObservableCollection<object> Columns => (ImpObservableCollection<object>)GetValue(ColumnsProperty);


        /// <summary>
        /// Collection of Highlights in the chart.
        /// </summary>
        public ImpObservableCollection<object> Highlights => (ImpObservableCollection<object>)GetValue(HighlightsProperty);

        /// <summary>
        /// Collection of AxisLabels in the chart.
        /// </summary>
        public ImpObservableCollection<object> AxisLabels => (ImpObservableCollection<object>)GetValue(AxisLabelsProperty);


        /// <summary>
        /// The currently selected Column
        /// </summary>
        public object SelectedColumn
        {
            get => (object)GetValue(SelectedColumnProperty);
            set => SetValue(SelectedColumnProperty, value);
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Columns'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable ColumnsSource
        {
            get => (IEnumerable)GetValue(ColumnsSourceProperty);
            set => SetValue(ColumnsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each column item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate ColumnItemTemplate
        {
            get => (DataTemplate)GetValue(ColumnItemTemplateProperty);
            set => SetValue(ColumnItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector ColumnItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ColumnItemTemplateSelectorProperty);
            set => SetValue(ColumnItemTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each column item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style ColumnItemContainerStyle
        {
            get => (Style)GetValue(ColumnItemContainerStyleProperty);
            set => SetValue(ColumnItemContainerStyleProperty, value);
        }

        private object _selectedColumn;

        private static void OnSelectedColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointAndFigureChartControl nv = d as PointAndFigureChartControl;
            // Set the internal _SelectedColumn value before updating the ListBox selected Item
            // so that the SelectedItem changed event can check to see wether it should NOT set the value.
            try
            {
                nv._selectedColumn = e.NewValue;
                nv.SetSelectedColumn(e.NewValue);
            }
            catch
            {
                // Just swallow the exception of if the column doesn't exist.
            }
        }

        private void SetSelectedColumn(object value)
        {
            // It can be null if a column is deselected, which it always is before the next one is selected.
            if (value != null)
            {
                if (_columnItemsControl.SelectedItem != null && value.Equals(_columnItemsControl.SelectedItem))
                {
                    return;
                }
                if (_columnItemsControl != null)
                {
                    _columnItemsControl.SelectedItem = value;
                }
                else
                {
                    if (_initialSelectedColumns == null)
                    {
                        _initialSelectedColumns = new List<object>();
                    }

                    _initialSelectedColumns.Clear();
                    _initialSelectedColumns.Add(value);
                }
            }
        }

        private static object CoerceSelectedColumnValue(DependencyObject d, object value)
        {
            PointAndFigureChartControl nv = d as PointAndFigureChartControl;
            // return nv._SelectedColumn;
            // return nv.GetSelectedColumn();
            return nv.Columns.Where(n => n == value).FirstOrDefault();
        }

        /// <summary>
        /// A list of selected nodes.
        /// </summary>
        public IList SelectedColumns
        {
            get
            {
                if (_columnItemsControl != null)
                {
                    return _columnItemsControl.SelectedItems;
                }
                else
                {
                    if (_initialSelectedColumns == null)
                    {
                        _initialSelectedColumns = new List<object>();
                    }

                    return _initialSelectedColumns;
                }
            }
        }

        /// <summary>
        /// Clear the selection.
        /// </summary>
        public void SelectNone()
        {
            this.SelectedColumns.Clear();
        }

        /// <summary>
        /// An event raised when the columns selected in the PointAndFigureChart has changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Set to 'true' to enable the clearing of selection when empty space is clicked.
        /// This is set to 'true' by default.
        /// </summary>
        public bool IsClearSelectionOnEmptySpaceClickEnabled
        {
            get => (bool)GetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty);
            set => SetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty, value);
        }

        /// <summary>
        /// The chart grid size.
        /// This is set to '5' by default.
        /// </summary>
        public double GridSize
        {
            get => (double)GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }


        /// <summary>
        /// A reference to the collection that is the source used to populate 'Highlights'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable HighlightsSource
        {
            get => (IEnumerable)GetValue(HighlightsSourceProperty);
            set => SetValue(HighlightsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector HighlightItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(HighlightItemTemplateSelectorProperty);
            set => SetValue(HighlightItemTemplateSelectorProperty, value);
        }


        /// <summary>
        /// A reference to the collection that is the source used to populate 'AxisLabels'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable AxisLabelsSource
        {
            get => (IEnumerable)GetValue(AxisLabelsSourceProperty);
            set => SetValue(AxisLabelsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector AxisLabelItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(AxisLabelItemTemplateSelectorProperty);
            set => SetValue(AxisLabelItemTemplateSelectorProperty, value);
        }

        #region Private methods ...
        /// <summary>
        /// Event raised when a new collection has been assigned to the 'ColumnsSource' property.
        /// </summary>
        private static void ColumnsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointAndFigureChartControl c = (PointAndFigureChartControl)d;

            //
            // Clear 'Columns'.
            //
            c.Columns.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(c.ColumnsSource_CollectionChanged);
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    //
                    // Populate 'Columns' from 'ColumnsSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.Columns.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(c.ColumnsSource_CollectionChanged);
                }
            }
        }

        /// <summary>
        /// Event raised when a column has been added to or removed from the collection assigned to 'ColumnsSource'.
        /// </summary>
        private void ColumnsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'ColumnsSource' has been cleared, also clear 'Columns'.
                //
                Columns.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'ColumnsSource' also remove it from 'Columns'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        Columns.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'ColumnsSource' also add it to 'Columns'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        Columns.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'HighlightsSource' property.
        /// </summary>
        private static void HighlightsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointAndFigureChartControl c = (PointAndFigureChartControl)d;

            //
            // Clear 'Highlights'.
            //
            c.Highlights.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(c.HighlightsSource_CollectionChanged);
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    //
                    // Populate 'Highlights' from 'HighlightsSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.Highlights.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(c.HighlightsSource_CollectionChanged);
                }
            }
        }

        /// <summary>
        /// Event raised when a column has been added to or removed from the collection assigned to 'HighlightsSource'.
        /// </summary>
        private void HighlightsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'HighlightsSource' has been cleared, also clear 'Highlights'.
                //
                Highlights.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'HighlightsSource' also remove it from 'Highlights'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        Highlights.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'HighlightsSource' also add it to 'Highlights'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        Highlights.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'HighlightsSource' property.
        /// </summary>
        private static void AxisLabelsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointAndFigureChartControl c = (PointAndFigureChartControl)d;

            //
            // Clear 'AxisLabels'.
            //
            c.AxisLabels.Clear();

            if (e.OldValue != null)
            {
                var notifyCollectionChanged = e.OldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(c.AxisLabelsSource_CollectionChanged);
                }
            }

            if (e.NewValue != null)
            {
                var enumerable = e.NewValue as IEnumerable;
                if (enumerable != null)
                {
                    //
                    // Populate 'AxisLabels' from 'AxisLabelsSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.AxisLabels.Add(obj);
                    }
                }

                var notifyCollectionChanged = e.NewValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(c.AxisLabelsSource_CollectionChanged);
                }
            }
        }

        /// <summary>
        /// Event raised when a column has been added to or removed from the collection assigned to 'HighlightsSource'.
        /// </summary>
        private void AxisLabelsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'AxisLabelsSource' has been cleared, also clear 'AxisLabels'.
                //
                AxisLabels.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'AxisLabelsSource' also remove it from 'AxisLabels'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        AxisLabels.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'AxisLabelsSource' also add it to 'AxisLabels'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        AxisLabels.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Called after the visual tree of the control has been built.
        /// Search for and cache references to named parts defined in the XAML control template for NetworkView.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //
            // Cache the parts of the visual tree that we need access to later.
            //

            this._columnItemsControl = (PointAndFigureColumnItemsControl)this.Template.FindName("PART_ColumnItemsControl", this);
            if (this._columnItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_ColumnItemsControl' in the visual tree for 'PointAndFigureChartControl'.");
            }

            //
            // Synchronize initial selected nodes to the NodeItemsControl.
            //
            if (this._initialSelectedColumns != null && this._initialSelectedColumns.Count > 0)
            {
                foreach (var node in this._initialSelectedColumns)
                {
                    this._columnItemsControl.SelectedItems.Add(node);
                }
            }

            this._initialSelectedColumns = null; // Don't need this any more.

            this._columnItemsControl.SelectionChanged += new SelectionChangedEventHandler(columnItemsControl_SelectionChanged);


        }

        /// <summary>
        /// Event raised when the selection in 'columnItemsControl' changes.
        /// </summary>
        private void columnItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, new SelectionChangedEventArgs(ListBox.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
            }
            // The selected item matches the internal value DO NOT set it.
            if ((_selectedColumn == null && _columnItemsControl.SelectedItem == null) || (_selectedColumn != null && _selectedColumn.Equals(_columnItemsControl.SelectedItem)))
            {
                return;
            }
            // Update the selected column.
            this.SelectedColumn = _columnItemsControl.SelectedItem;
        }

        #endregion

    }
}
