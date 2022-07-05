using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;
using PnFDesktop.Services;
using PnFDesktop.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Controls;

namespace PnFDesktop.ViewCharts
{
    public class PointAndFigureChartViewModel : PaneViewModel, IPointAndFigureChartViewModel
    {
        #region Internal data members ...

        private IChartLayoutManager _chartLayoutManager = new ChartLayoutManager();

        ///
        /// The current scale at which the content is being viewed.
        /// 
        private double _contentScale = 1;

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double _contentOffsetX = 0;

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double _contentOffsetY = 0;

        ///
        /// The width of the content (in content coordinates).
        /// 
        private double _contentWidth = 3000;

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        private double _contentHeight = 2000;

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double _contentViewportWidth = 0;

        ///
        /// The height of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double _contentViewportHeight = 0;

        #endregion Internal Data Members
        private PnFChart? _chart;

        /// <summary>
        /// Sets and gets the Chart property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFChart? Chart
        {
            get => _chart;
            set
            {
                if (SetProperty(ref _chart, value))
                {
                    OnPropertyChanged("ChartId");
                }
            }
        }

        public Guid ChartId => _chart.Id;

        private double _gridSize = 5d;

        public double GridSize
        {
            get => _gridSize;
            set => SetProperty(ref _gridSize, value);
        }

        private double _leftPadding = 30d;

        public double LeftPadding
        {
            get => _leftPadding;
            set => SetProperty(ref _leftPadding, value);
        }

        private double _rightPadding = 30d;

        public double RightPadding
        {
            get => _rightPadding;
            set => SetProperty(ref _rightPadding, value);
        }

        private double _topPadding = 30d;


        public double TopPadding
        {
            get => _topPadding;
            set => SetProperty(ref _topPadding, value);
        }


        private double _bottomPadding = 30d;

        public double BottomPadding
        {
            get => _bottomPadding;
            set => SetProperty(ref _bottomPadding, value);
        }

        #region Layout and zoom control properties and methods...

        private double _viewportWidth = double.NaN;

        /// <summary>
        /// Sets and gets the ViewportWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ViewportWidth
        {
            get => _viewportWidth;

            private set
            {
                if (SetProperty(ref _viewportWidth, value))
                {
                    OnPropertyChanged("MinViewportScale");
                }
            }
        }

        private double _viewportHeight = double.NaN;

        /// <summary>
        /// Sets and gets the ViewportHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ViewportHeight
        {
            get => _viewportHeight;

            private set
            {
                if (SetProperty(ref _viewportHeight, value))
                {
                    OnPropertyChanged("MinViewportScale");
                }

            }
        }


        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double ContentScale
        {
            get => _contentScale;
            set => SetProperty(ref _contentScale, value);
        }

        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double MinViewportScale
        {
            get
            {
                if (!ViewportWidth.Equals(double.NaN) && !ViewportHeight.Equals(double.NaN))
                {
                    return Math.Min(ViewportWidth / ContentWidth, ViewportHeight / ContentHeight);
                }
                else
                {
                    return 0.05;
                }
            }
        }

        ///
        /// The maximum port scale
        /// 
        public double MaxViewportScale
        {
            get
            {
                if (!ViewportWidth.Equals(double.NaN) && !ViewportHeight.Equals(double.NaN))
                {
                    return Math.Max(ViewportWidth / 50.0 * _chartLayoutManager.GridSize, ViewportHeight / 50.0 * _chartLayoutManager.GridSize);
                }
                else
                {
                    return 0.05;
                }
            }
        }

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetX
        {
            get => _contentOffsetX;
            set => SetProperty(ref _contentOffsetX, value);
        }

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetY
        {
            get => _contentOffsetY;
            set => SetProperty(ref _contentOffsetY, value);
        }

        ///
        /// The width of the content (in content coordinates).
        /// 
        public double ContentWidth
        {
            get => _contentWidth;
            set
            {
                if (SetProperty(ref _contentWidth, value))
                {
                    OnPropertyChanged("MinViewportScale");
                }
            }
        }

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        public double ContentHeight
        {
            get => _contentHeight;
            set
            {
                if (SetProperty(ref _contentHeight, value))
                {
                    OnPropertyChanged("MinViewportScale");
                }
            }
        }

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportWidth
        {
            get => _contentViewportWidth;
            set => SetProperty(ref _contentViewportWidth, value);
        }

        ///
        /// The heigth of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportHeight
        {
            get => _contentViewportHeight;
            set => SetProperty(ref _contentViewportHeight, value);
        }


        public void SetViewportSize(double viewportWidth, double viewportHeight)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            OnPropertyChanged("MinViewportScale");
            OnPropertyChanged("MaxViewportScale");
        }
        #endregion

        public ImpObservableCollection<PointAndFigureColumnViewModel> Columns { get; } =
            new ImpObservableCollection<PointAndFigureColumnViewModel>();

        private PointAndFigureColumnViewModel _selectedColumn;
        public PointAndFigureColumnViewModel SelectedColumn
        {
            get => _selectedColumn;
            set
            {
                if (SetProperty(ref _selectedColumn, value))
                {
                    if (_selectedColumn != null)
                    {
                        WeakReferenceMessenger.Default.Send(new ActivePointAndFigureColumnChangedMessage(this, _selectedColumn.Column));
                    }
                }
            }
        }

        public ImpObservableCollection<PointAndFigureHighlightViewModel> Highlights { get; } =
    new ImpObservableCollection<PointAndFigureHighlightViewModel>();
        public ImpObservableCollection<PointAndFigureAxisLabelViewModel> AxisLabels { get; } =
    new ImpObservableCollection<PointAndFigureAxisLabelViewModel>();

        public new string Title
        {
            get
            {
                if (Chart != null && !string.IsNullOrEmpty(Chart.Name))
                {
                    return Chart.Name;
                }
                else
                {
                    return "A Chart";
                }
            }
        }


        private string _tooltip = null;

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }


        #region Control property ...

        private UserControl _control = null;

        /// <summary>
        /// Sets and gets the Control property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public UserControl Control
        {
            get => _control;
            set => SetProperty(ref _control, value);
        }
        #endregion

        [PreferredConstructor]
        public PointAndFigureChartViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.Chart = new DesignDataService().GetPointAndFigureChart("Test", 5, 3);
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("1000", AxisLabelLocation.Left, 0d, 10d));
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("2000", AxisLabelLocation.Left, 0d, 20d));
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("3000", AxisLabelLocation.Left, 0d, 30d));
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("4000", AxisLabelLocation.Left, 0d, 40d));
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("5000", AxisLabelLocation.Left, 0d, 50d));
                this.AxisLabels.Add(new PointAndFigureAxisLabelViewModel("6000", AxisLabelLocation.Left, 0d, 60d));
            }
        }

        public PointAndFigureChartViewModel(PnFChart chart)
        {
            Chart = chart;
            ContentId = $"{Constants.PointAndFigureChart}_{chart.Id}";
            _chartLayoutManager.Initialize(chart, 5d, 10d, 25d);

            ContentWidth = _chartLayoutManager.SheetWidth;
            ContentHeight = _chartLayoutManager.SheetHeight;
            OnPropertyChanged("MaxViewportScale");

            Control = new PointAndFigureChartView(this);
        }

        public void BuildChartData()
        {
            Highlights.Clear();
            AxisLabels.Clear();
            AddColumns(out int minRowIndex, out int maxRowIndex);
            AddRowData(minRowIndex, maxRowIndex);
        }

        #region Helper methods ...
        private void AddColumns(out int minRowIndex, out int maxRowIndex)
        {
            minRowIndex = int.MaxValue;
            maxRowIndex = int.MinValue;
            Columns.Clear();
            foreach (PnFColumn column in this.Chart.Columns.OrderBy(c => c.Index))
            {
                if (column.StartAtIndex < minRowIndex) minRowIndex = column.StartAtIndex;
                if (column.EndAtIndex < minRowIndex) minRowIndex = column.EndAtIndex;
                if (column.StartAtIndex > maxRowIndex) maxRowIndex = column.StartAtIndex;
                if (column.EndAtIndex > maxRowIndex) maxRowIndex = column.EndAtIndex;

                PointAndFigureColumnViewModel columnVm = new PointAndFigureColumnViewModel(column, _chartLayoutManager);
                Columns.Add(columnVm);
                if (column.ContainsNewYear && column.EndAt.HasValue)
                {
                    ColumnData colData = _chartLayoutManager.GetColumnData(column.Index);
                    AxisLabels.Add(new PointAndFigureAxisLabelViewModel($"{column.EndAt.Value.Year}", AxisLabelLocation.Bottom, colData.BottomLabel.X, colData.BottomLabel.Y));
                    Highlights.Add(new PointAndFigureHighlightViewModel(colData.ColumnHighlight, Orientation.Vertical));
                }
            }
        }

        private void AddRowData(int minRowIndex, int maxRowIndex)
        {
            if (minRowIndex != int.MaxValue && maxRowIndex != int.MinValue)
            {
                if (this.Chart.PriceScale == PnFChartPriceScale.Normal)
                {
                    // Add row backgrounds
                    for (int i = minRowIndex; i <= maxRowIndex; i++)
                    {
                        RowData rowData = _chartLayoutManager.GetRowData(i);
                        if (i % 5 == 0)
                        {
                            Highlights.Add(new PointAndFigureHighlightViewModel(rowData.RowHighlight, Orientation.Horizontal));
                        }
                        AxisLabels.Add(new PointAndFigureAxisLabelViewModel($"{(i * this.Chart.BoxSize):F}", AxisLabelLocation.Left, rowData.LeftLabel.X, rowData.LeftLabel.Y));
                        AxisLabels.Add(new PointAndFigureAxisLabelViewModel($"{(i * this.Chart.BoxSize):F}", AxisLabelLocation.Right, rowData.RightLabel.X, rowData.RightLabel.Y));
                    }
                }
                else
                {
                    // Logarithmic
                    double increment = Math.Log(1d + (this.Chart!.BoxSize!.Value * 0.01));    // Calculate the boxsize and a percentage incrementor (e,g, 2 => 1.02)
                    for (int i = minRowIndex; i <= maxRowIndex; i++)
                    {
                        RowData rowData = _chartLayoutManager.GetRowData(i);
                        if (i % 5 == 0) // Add a row and labels every 5 rows
                        {
                            double rowValue = Math.Exp(this.Chart!.BaseValue!.Value + (increment * i));
                            Highlights.Add(new PointAndFigureHighlightViewModel(rowData.RowHighlight, Orientation.Horizontal));
                            AxisLabels.Add(new PointAndFigureAxisLabelViewModel($"{rowValue:F}", AxisLabelLocation.Left, rowData.LeftLabel.X, rowData.LeftLabel.Y));
                            AxisLabels.Add(new PointAndFigureAxisLabelViewModel($"{rowValue:F}", AxisLabelLocation.Right, rowData.RightLabel.X, rowData.RightLabel.Y));
                        }
                    }
                }
            }

        }

        #endregion
    }
}
