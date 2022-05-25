using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PnFDesktop.ViewModels
{
    public enum InitialPaneLocation
    {
        Left,
        Right,
        Bottom
    }

    public class PaneViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the PaneViewModel class.
        /// </summary>
        public PaneViewModel()
        {
        }


        private InitialPaneLocation _initialPaneLocation = InitialPaneLocation.Left;

        /// <summary>
        /// Sets and gets the InitialPaneLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public InitialPaneLocation InitialPaneLocation
        {
            get => _initialPaneLocation;
            set => SetProperty(ref _initialPaneLocation, value);
        }


        #region Title property ...

        private string _title = string.Empty;

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        #endregion

        #region ContentId property ...

        private string _contentId = string.Empty;

        /// <summary>
        /// Sets and gets the ContentId property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ContentId
        {
            get => _contentId;
            set => SetProperty(ref _contentId, value);
        }
        #endregion

        #region IsSelected property ...


        private bool _isSelected = false;

        /// <summary>
        /// Sets and gets the IsSelected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        #endregion

        #region IsActive property ...

        private bool _isActive = false;

        /// <summary>
        /// Sets and gets the IsActive property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        #endregion


        #region IsBusy property ...

        private bool _isBusy = false;

        /// <summary>
        /// Sets and gets the IsBusy property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion

    }
}
