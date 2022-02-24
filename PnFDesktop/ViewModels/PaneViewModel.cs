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

        /// <summary>
        /// The <see cref="InitialPaneLocation" /> property's name.
        /// </summary>
        public const string InitialPaneLocationPropertyName = "InitialPaneLocation";

        private InitialPaneLocation _InitialPaneLocation = InitialPaneLocation.Left;

        /// <summary>
        /// Sets and gets the InitialPaneLocation property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public InitialPaneLocation InitialPaneLocation
        {
            get
            {
                return _InitialPaneLocation;
            }

            set
            {
                if (_InitialPaneLocation == value)
                {
                    return;
                }

                _InitialPaneLocation = value;
                OnPropertyChanged(InitialPaneLocationPropertyName);
            }
        }

        #region Title property ...
        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _Title = string.Empty;

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }

            set
            {
                if (_Title == value)
                {
                    return;
                }

                _Title = value;
                OnPropertyChanged(TitlePropertyName);
            }
        }
        #endregion

        #region ContentId property ...
        /// <summary>
        /// The <see cref="ContentId" /> property's name.
        /// </summary>
        public const string ContentIdPropertyName = "ContentId";

        private string _ContentId = string.Empty;

        /// <summary>
        /// Sets and gets the ContentId property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ContentId
        {
            get
            {
                return _ContentId;
            }

            set
            {
                if (_ContentId == value)
                {
                    return;
                }

                _ContentId = value;
                OnPropertyChanged(ContentIdPropertyName);
            }
        }
        #endregion

        #region IsSelected property ...

        /// <summary>
        /// The <see cref="IsSelected" /> property's name.
        /// </summary>
        public const string IsSelectedPropertyName = "IsSelected";

        private bool _IsSelected = false;

        /// <summary>
        /// Sets and gets the IsSelected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }

            set
            {
                if (_IsSelected == value)
                {
                    return;
                }

                _IsSelected = value;
                OnPropertyChanged(IsSelectedPropertyName);
            }
        }
        #endregion

        #region IsActive property ...
        /// <summary>
        /// The <see cref="IsActive" /> property's name.
        /// </summary>
        public const string IsActivePropertyName = "IsActive";

        private bool _IsActive = false;

        /// <summary>
        /// Sets and gets the IsActive property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }

            set
            {
                if (_IsActive == value)
                {
                    return;
                }

                _IsActive = value;
                OnPropertyChanged(IsActivePropertyName);
            }
        }

        #endregion
    }
}
