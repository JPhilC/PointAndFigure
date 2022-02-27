﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public class PointAndFigureBoxViewModel:ObservableObject
    {
        #region Properties ...

        private PnFBox _box;

        /// <summary>
        /// Sets and gets the Port property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFBox Box
        {
            get => _box;
            set => SetProperty(ref _box, value);
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
        /// The Z index of the box.
        /// </summary>
        private int _zIndex;

        /// <summary>
        /// The Z index of the box.
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        #endregion

        public DrawingImage Image
        {
            get
            {
                DrawingImage image = Application.Current.TryFindResource(ArtworkKey) as DrawingImage;
                if (image == null)
                {
                    MessageLog.LogMessage(this, LogType.Error, $"The artwork is missing for key '{ArtworkKey}'.");
                    image = Application.Current.TryFindResource("UnknownPort") as DrawingImage;
                }
                return image;
            }
        }

        private string ArtworkKey
        {
            get
            {
                string key = "UnknownPort";
                switch (Box.BoxType)
                {
                    case PnFBoxType.O:
                        key = "OBox5X5";
                        break;
                    case PnFBoxType.X:
                        key = "XBox5X5";
                        break;
                    default:
                        break;
                }
                return key;

            }
        }

        public PointAndFigureBoxViewModel(PnFBox box, float chartGridSize, double reversingYfactor)
        {
            Box = box;
            _x = Box.Column.Index * chartGridSize;
            _y = reversingYfactor -(Box.Index * chartGridSize);
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
        }

    }
}
