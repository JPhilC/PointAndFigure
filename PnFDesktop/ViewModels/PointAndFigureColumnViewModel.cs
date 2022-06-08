using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Accessibility;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;

namespace PnFDesktop.ViewModels
{
    public class PointAndFigureColumnViewModel: ObservableObject
    {
        #region Properties ...
        private PnFColumn _column;

        /// <summary>
        /// Sets and gets the Item property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFColumn Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        /// <summary>
        /// The X coordinate for the position of the column.
        /// </summary>
        private double _x;

        /// <summary>
        /// The X coordinate for the position of the column.
        /// </summary>
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        /// <summary>
        /// The Y coordinate for the position of the column.
        /// </summary>
        private double _y;

        /// <summary>
        /// The Y coordinate for the position of the column.
        /// </summary>
        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        /// <summary>
        /// The Y coordinate for the position of the bullish support line.
        /// </summary>
        private double _bullishSupportY;

        /// <summary>
        /// The Y coordinate for the position of the bullish support line.
        /// </summary>
        public double BullishSupportY
        {
            get => _bullishSupportY;
            set => SetProperty(ref _bullishSupportY, value);
        }

        private bool _showBullishSupportImage;

        /// <summary>
        /// The Y coordinate for the position of the bullish support line.
        /// </summary>
        public bool ShowBullishSupportImage
        {
            get => _showBullishSupportImage;
            set => SetProperty(ref _showBullishSupportImage, value);
        }

        private string _artworkKey = "BullishSupport5X5";

        public DrawingImage BullishSupportImage
        {
            get
            {
                DrawingImage image = Application.Current.TryFindResource(_artworkKey) as DrawingImage;
                if (image == null)
                {
                    MessageLog.LogMessage(this, LogType.Error, $"The artwork is missing for key '{_artworkKey}'.");
                    image = Application.Current.TryFindResource("UnknownPort") as DrawingImage;
                }
                return image;
            }
        }


        private Size _size = new Size(0, 0);

        public string Tooltip => Column.GetTooltip();

        /// <summary>
        /// The size of the node.
        /// 
        /// Important Note: 
        ///     The size of a node in the UI is not determined by this property!!
        ///     Instead the size of a node in the UI is determined by the data-template for the Node class.
        ///     When the size is computed via the UI it is then pushed into the view-model
        ///     so that our application code has access to the size of a node.
        /// </summary>
        public Size Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }


        private bool _isSelected = false;
        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// List of boxes the column.
        /// </summary>
        public ImpObservableCollection<PointAndFigureBoxViewModel> Boxes { get; }= new ImpObservableCollection<PointAndFigureBoxViewModel>();
        #endregion

        public PointAndFigureColumnViewModel(PnFColumn column, IChartLayoutManager layoutManager)
        {
            _column = column;
            _x = layoutManager.GetColumnXCoordinate(column.Index);
            if (column.Boxes.Any())
            {
                Size = AddBoxes(layoutManager, out int minIndex, out int maxIndex);
                _y = layoutManager.GetRowYCoordinate(maxIndex);
                ShowBullishSupportImage = Column.ShowBullishSupport;
            }
            else
            {
                _y = 0d;
                _bullishSupportY = 0d;
                ShowBullishSupportImage = false;
            }
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
            OnPropertyChanged("BullishSupportY");

        }

        /// <summary>
        /// Add the boxes, remembering that the coordinates are relative to the column.
        /// </summary>
        /// <param name="layoutManager"></param>
        /// <param name="minIndex"></param>
        /// <param name="maxIndex"></param>
        /// <returns></returns>
        private Size AddBoxes(IChartLayoutManager layoutManager, out int minIndex, out int maxIndex)
        {
            PointAndFigureBoxViewModel boxVm = null;
            int i = 0;
            foreach (var box in Column.Boxes.OrderByDescending(b=>b.Index))
            {
                boxVm = new PointAndFigureBoxViewModel(box, i * layoutManager.GridSize);
                this.Boxes.Add(boxVm);
                i++;
            }

            minIndex = Boxes.Min(b => b.Box.Index);
            maxIndex = Boxes.Max(b => b.Box.Index);
            if (boxVm != null)
            {
                this.BullishSupportY = Boxes[0].Y + ((maxIndex - Column.BullSupportIndex) * layoutManager.GridSize);
            }

            return new Size(layoutManager.GridSize, Boxes.Count * layoutManager.GridSize);
        }

    }
}
