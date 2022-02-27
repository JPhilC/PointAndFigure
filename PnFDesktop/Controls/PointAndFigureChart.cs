using System;
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
    public class PointAndFigureChartControl: Control
    {
        #region Dependency Property/Event Definitions

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

        #region Private Data Members

        /// <summary>
        /// Cached reference to the ColumnItemsControl in the visual-tree.
        /// </summary>
        private PointAndFigureColumnItemsControl _columnItemsControl;

        /// <summary>
        /// Cached the currently selected column.
        /// </summary>
        private object _initialSelectedColumn;


        #endregion

        public PointAndFigureChartControl()
        {
            //
            // Create a collection to contain columns.
            //
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
        /// Collection of columns in the chart.
        /// </summary>
        public ImpObservableCollection<object> Columns => (ImpObservableCollection<object>)GetValue(ColumnsProperty);

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
                    _initialSelectedColumn = value;
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
                throw new ApplicationException("Failed to find 'PART_ColumnItemsControl' in the visual tree for 'NetworkView'.");
            }

            //
            // Synchronize initial selected columns to the ColumnItemsControl.
            //
            if (this._initialSelectedColumn != null)
            {
                
                    this._columnItemsControl.SelectedItem = this._initialSelectedColumn;
                
            }

            this._initialSelectedColumn = null; // Don't need this any more.

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

        /// <summary>
        /// Find the max ZIndex of all the columns and annotations.
        /// </summary>
        internal int FindMaxZIndex()
        {
            if (this._columnItemsControl == null)
            {
                return 0;
            }

            int maxZ = 0;

            if (this._columnItemsControl != null)
            {
                for (int columnIndex = 0; ; ++columnIndex)
                {
                    PointAndFigureColumnItem columnItem = (PointAndFigureColumnItem)this._columnItemsControl.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                    if (columnItem == null)
                    {
                        break;
                    }

                    if (columnItem.ZIndex > maxZ)
                    {
                        maxZ = columnItem.ZIndex;
                    }
                }
            }

            return maxZ;
        }

        /// <summary>
        /// Find the ColumnItem UI element that is associated with 'column'.
        /// 'column' can be a view-model object, in which model the visual-tree
        /// is searched for the associated ColumnItem.
        /// Otherwise 'column' can actually be a 'ColumnItem' in which case it is 
        /// simply returned.
        /// </summary>
        public PointAndFigureColumnItem FindAssociatedColumnItem(object column)
        {
            PointAndFigureColumnItem columnItem = column as PointAndFigureColumnItem;
            if (columnItem == null)
            {
                columnItem = _columnItemsControl.FindAssociatedColumnItem(column);
            }
            return columnItem;
        }

        #endregion

    }
}
