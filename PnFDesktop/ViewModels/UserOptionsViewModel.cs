using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using PnFDesktop.Classes;

namespace PnFDesktop.ViewModels
{
    public class UserOptionsViewModel : ObservableObject
    {
        private bool _isBusy = false;

        /// <summary>
        /// Sets and gets the MyProperty property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;

            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    SaveChangesAndCloseCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ApplicationOptionsManager.ApplicationOptions Options => ApplicationOptionsManager.Options;

        public UserOptionsViewModel()
        {

        }

        protected bool OnSaveCommand()
        {
            return SaveOptions();
        }

        public bool SaveOptions()
        {
            ApplicationOptionsManager.SaveOptions();
            return true;
        }

        #region Actions ....
        public Action SaveAndCloseAction { get; set; }
        public Action CancelAndCloseAction { get; set; }
        #endregion

        #region Relay commands ....
        /// <summary>
        /// Gets the SaveChangesAndCloseCommand.
        /// </summary>

        private RelayCommand _saveChangesAndCloseCommand;

        /// <summary>
        /// Gets the SaveChangesAndCloseCommand.
        /// </summary>
        public RelayCommand SaveChangesAndCloseCommand
        {
            get
            {
                return _saveChangesAndCloseCommand
                    ?? (_saveChangesAndCloseCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (OnSaveCommand())
                                              {
                                                  // Don't need to do anything here as the assumption is that the properties
                                                  // and bound and therefore already saved.
                                                  if (this.SaveAndCloseAction != null)
                                                  {
                                                      SaveAndCloseAction();
                                                  }
                                              }
                                          }, () =>
                                          {
#if DEBUG
                                              return (!IsBusy);
#else
                                            return (!IsBusy && (_Data == null || (_Data != null) && !_Data.HasErrors));
#endif
                                          }));
            }
        }


        #endregion

    }
}
