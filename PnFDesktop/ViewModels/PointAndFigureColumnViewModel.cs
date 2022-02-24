using System;
using System.Configuration;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Classes;

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
        /// The Z index of the column.
        /// </summary>
        private int _zIndex;

        /// <summary>
        /// The Z index of the column.
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        private Size _size = new Size(0, 0);

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
            set
            {
                if (_size == value)
                {
                    return;
                }
                if (SizeChanging != null)
                {
                    SizeChanging(this, EventArgs.Empty);
                }

                _size = value;

                if (SizeChanged != null)
                {
                    SizeChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event raised when the size of the node is about to change.
        /// </summary>
        public event EventHandler<EventArgs> SizeChanging;

        /// <summary>
        /// Event raised when the size of the node is changed.
        /// The size will change when the UI has determined its size based on the contents
        /// of the nodes data-template.  It then pushes the size through to the view-model
        /// and this 'SizeChanged' event occurs.
        /// </summary>
        public event EventHandler<EventArgs> SizeChanged;

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

        public PointAndFigureColumnViewModel(PnFColumn column, float chartGridSize)
        {
            _column = column;
            _x = column.Index * chartGridSize;
            _y = 0f;
            AddBoxes(chartGridSize);
        }

        private void AddBoxes(float chartGridSize)
        {
            foreach (var box in Column.Boxes)
            {
                this.Boxes.Add(new PointAndFigureBoxViewModel(box, chartGridSize));
            }
        }

    }
}
