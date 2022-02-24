using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.ViewModels
{
    public class ToolViewModel : PaneViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ToolViewModel class.
        /// </summary>
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;

        }



        public string Name
        {
            get;
            private set;
        }

        #region IsVisible property ...
        /// <summary>
        /// The <see cref="IsVisible" /> property's name.
        /// </summary>
        public const string IsVisiblePropertyName = "IsVisible";

        private bool _IsVisible = true;

        /// <summary>
        /// Sets and gets the IsVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }

            set
            {
                if (_IsVisible == value)
                {
                    return;
                }

                _IsVisible = value;
                OnPropertyChanged(IsVisiblePropertyName);
            }
        }
        #endregion


    }
}
