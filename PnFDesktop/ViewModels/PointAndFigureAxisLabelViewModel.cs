using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Accessibility;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public enum AxisLabelLocation
    {
        [Description("Y-axis on the left.")]
        Left,
        [Description("Y-axis on the right.")]
        Right,
        [Description("X-axis on the bottom.")]
        Bottom
    }
    public class PointAndFigureAxisLabelViewModel:ObservableObject
    {
        #region Properties ...

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private AxisLabelLocation _location;
        public AxisLabelLocation Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
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
        #endregion
        public PointAndFigureAxisLabelViewModel(string text, AxisLabelLocation location, double x, double y)
        {
            _x = x;
            _y = y;
            _text = text;
            _location = location;
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
            OnPropertyChanged("Text");
            OnPropertyChanged("Location");
        }

    }
}
