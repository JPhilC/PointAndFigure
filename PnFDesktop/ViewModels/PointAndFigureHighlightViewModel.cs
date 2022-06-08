using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Accessibility;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public class PointAndFigureHighlightViewModel:ObservableObject
    {
        #region Properties ...
        private Orientation _orientation;

        /// <summary>
        /// The X coordinate for the position of the box.
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set => SetProperty(ref _orientation, value);
        }


        /// <summary>
        /// The X coordinate for the position of the box.
        /// </summary>
        private double _x;

        /// <summary>
        /// The X coordinate for the position of the box.
        /// </summary>
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        /// <summary>
        /// The Y coordinate for the position of the box.
        /// </summary>
        private double _y;

        /// <summary>
        /// The Y coordinate for the position of the box.
        /// </summary>
        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        /// <summary>
        /// The width for the row background.
        /// </summary>
        private double _width;

        /// <summary>
        /// The width of the row background.
        /// </summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// The height of the row background.
        /// </summary>
        private double _height;

        /// <summary>
        /// The height of the row background.
        /// </summary>
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
        #endregion


        public PointAndFigureHighlightViewModel(Highlight data, Orientation orientation)
        {
            _x = data.X;
            _y = data.Y;
            _width = data.Width;
            _height = data.Height;
            _orientation = orientation;
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
            OnPropertyChanged("Orientation");
        }

    }
}
