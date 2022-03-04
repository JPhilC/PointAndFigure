using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;
using PnFDesktop.Services;
using PnFDesktop.ViewModels;
using PnFDesktop.Views;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes.Messaging;

namespace PnFDesktop.ViewCharts
{
    public class PointAndFigureChartViewModel : PaneViewModel, IPointAndFigureChartViewModel
    {
        #region Internal data members ...
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
        private PnFChart _chart;

        /// <summary>
        /// Sets and gets the Chart property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFChart Chart
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

        private double _topPadding = 30d;

        public double TopPadding
        {
            get => _topPadding;
            set => SetProperty(ref _topPadding, value);
        }

        #region Layout and zoom control properties and methods...
        private double _contentLeftLimit = 0.0;
        private double _contentRightLimit = 0.0;
        private double _contentTopLimit = 0.0;
        private double _contentBottomLimit = 0.0;

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
                    return Math.Max(ViewportWidth / ContentWidth, ViewportHeight / ContentHeight);
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
            set => SetProperty(ref _contentWidth, value);
        }

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        public double ContentHeight
        {
            get => _contentHeight;
            set => SetProperty(ref _contentHeight, value);
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
                CalculateLimits();
                AddColumns();
            }
        }

        public PointAndFigureChartViewModel(PnFChart chart)
        {
            Chart = chart;

            CalculateLimits();
            AddColumns();
            // At this stage the content is at least a margins width from the top and left
            // So the width and height are the right and bottom limit plus the margin.
            ContentHeight = _contentBottomLimit + TopPadding + Constants.ChartMargin;
            ContentWidth = _contentRightLimit + LeftPadding + Constants.ChartMargin;

            Control = new PointAndFigureChartView(this);
        }

        /// <summary>
        ///  Look for the Node sent in the message in the nodes list 
        /// </summary>
        /// <param name="selectedNode"></param>
        private void HandleColumnMessage(ActivePointAndFigureColumnChangedMessage message)
        {
            if (message.Sender != this)
            {
                PointAndFigureColumnViewModel columnVm = this.Columns.FirstOrDefault(n => n.Column.Id == message.Column.Id);
                SelectedColumn = columnVm;
            }
        }

        #region Helper methods ...
        private void AddColumns()
        {
            Columns.Clear();
            foreach (PnFColumn column in Chart.Columns)
            {
                PointAndFigureColumnViewModel columnVm = new PointAndFigureColumnViewModel(column, GridSize, _maxIndex);
                Columns.Add(columnVm);
            }
        }


        private int _maxIndex;

        /// <summary>
        /// Calculates the drawing limits and if necessary shifts the columns and annotations
        /// to get them back on the sheet.
        /// </summary>
        private void CalculateLimits()
        {

            double columnLeftLimit = 0d;
            double columnTopLimit = 0d;
            double columnRightLimit;
            double columnBottomLimit;
            //double annLeftLimit = 0.0;
            //double annTopLimit = 0.0;
            //double annRightLimit;
            //double annBottomLimit;
            int minColIndex = int.MaxValue;
            int maxColIndex = int.MinValue;
            int minBoxIndex = int.MaxValue;
            int maxBoxIndex = int.MinValue;
            _maxIndex = 0;

            if (Chart.Columns.Count > 0)
            {
                foreach (var column in Chart.Columns)
                {
                    minColIndex = Math.Min(minColIndex, column.Index);
                    maxColIndex = Math.Max(maxColIndex, column.Index);
                    foreach (var box in column.Boxes)
                    {
                        minBoxIndex = Math.Min(minBoxIndex, box.Index);
                        maxBoxIndex = Math.Max(maxBoxIndex, box.Index);
                        if (box.Index > _maxIndex) _maxIndex = box.Index;
                    }
                }
                columnLeftLimit = Math.Min((minColIndex * GridSize), 0.0);
                columnTopLimit = Math.Min((minBoxIndex * GridSize), 0.0);
                columnRightLimit = Math.Max((maxColIndex * GridSize), Constants.DefaultChartWidth);
                columnBottomLimit = Math.Max((maxBoxIndex * GridSize), Constants.DefaultChartHeight);

            }
            else
            {
                columnRightLimit = Constants.DefaultChartWidth;
                columnBottomLimit = Constants.DefaultChartHeight;
            }

            _contentLeftLimit = columnLeftLimit;
            _contentRightLimit = columnRightLimit;
            _contentTopLimit = columnTopLimit;
            _contentBottomLimit = columnBottomLimit;


            // Note: This block left in for when annotation support is added.
            //if (Chart.Annotations.Count > 0)
            //{
            //    annLeftLimit = Math.Min(Chart.Annotations.Select(n => n.LayoutXPosition).Min(), 0.0);
            //    annTopLimit = Math.Min(Chart.Annotations.Select(n => n.LayoutYPosition).Min(), 0.0);
            //    annRightLimit = Math.Max(Chart.Annotations.Select(n => n.LayoutXPosition).Max(), ApplicationOptionsManager.Options.DefaultSheetWidth);
            //    annBottomLimit = Math.Max(Chart.Annotations.Select(n => n.LayoutYPosition).Max(), ApplicationOptionsManager.Options.DefaultSheetHeight);
            //}
            //else
            //{
            //    annRightLimit = ApplicationOptionsManager.Options.DefaultSheetWidth;
            //    annBottomLimit = ApplicationOptionsManager.Options.DefaultSheetHeight;
            //}


            //_contentLeftLimit = Math.Min(annLeftLimit, columnLeftLimit);
            //_contentRightLimit = Math.Max(annRightLimit, columnRightLimit);
            //_contentTopLimit = Math.Min(annTopLimit, columnTopLimit);
            //_contentBottomLimit = Math.Max(annBottomLimit, columnBottomLimit);


            // Assuming that content can only grow east and south we need to move the content if some of it
            // is too far North and West.
            double xShift = 0.0;
            double yShift = 0.0;
            if (_contentLeftLimit < 0.0)
            {
                xShift = 0.0 - _contentLeftLimit;
            }
            if (_contentTopLimit < 0.0)
            {
                yShift = 0.0 - _contentTopLimit;
            }

            //if (xShift > 0.0 || yShift > 0.0)
            //{
            //    Chart.Columns.ToList().ForEach(n =>
            //    {
            //        n.LayoutXPosition = n.LayoutXPosition + xShift;
            //        n.LayoutYPosition = n.LayoutYPosition + yShift;
            //    });
            //    Chart.Annotations.ToList().ForEach(n =>
            //    {
            //        n.LayoutXPosition = n.LayoutXPosition + xShift;
            //        n.LayoutYPosition = n.LayoutYPosition + yShift;
            //    });
            //    _contentLeftLimit = _contentLeftLimit + xShift;
            //    _contentRightLimit = _contentRightLimit + xShift;
            //    _contentTopLimit = _contentTopLimit + yShift;
            //    _contentBottomLimit = _contentBottomLimit + yShift;
            //}
        }

        public void ResizeContent()
        {

            CalculateLimits();

            double contentHeight = _contentBottomLimit + Constants.ChartMargin;
            double contentWidth = _contentRightLimit + Constants.ChartMargin;
            bool raisePropertyChanged = false;

            if (contentHeight > this.ContentHeight)
            {
                this.ContentHeight = contentHeight;
                raisePropertyChanged = true;
            }
            if (contentWidth > this.ContentWidth)
            {
                this.ContentWidth = contentWidth;
                raisePropertyChanged = true;
            }
            if (raisePropertyChanged)
            {
                OnPropertyChanged("MinViewportScale");
            }
        }

        #endregion
    }
}
