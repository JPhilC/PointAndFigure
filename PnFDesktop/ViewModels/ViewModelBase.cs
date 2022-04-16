using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {

        #region Actions ....
        public Action? SaveAndCloseAction { get; set; }
        public Action? CancelAndCloseAction { get; set; }
        #endregion

        #region Relay commands ....
        private RelayCommand? _cancelChangesAndCloseCommand;

        /// <summary>
        /// Gets the SaveChangesAndCloseCommand.
        /// </summary>
        public RelayCommand CancelChangesAndCloseCommand
        {
            get
            {
                return _cancelChangesAndCloseCommand
                    ?? (_cancelChangesAndCloseCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (OnCancelCommand())
                                              {
                                                  if (this.CancelAndCloseAction != null)
                                                  {
                                                      CancelAndCloseAction();
                                                  }
                                              }
                                          }));
            }
        }

        /// <summary>
        /// Override this method to perform cancel code.
        /// Note: The data object is restored in the base class code.
        /// </summary>
        /// <returns>True if you want the cacncel command to close the window.</returns>
        protected virtual bool OnCancelCommand()
        {
            OnPropertyFormClosed(false);
            return true;
        }

        private RelayCommand? _saveChangesAndCloseCommand;

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
                                          }));
            }
        }

        /// <summary>
        /// Override this method to perform save code.
        /// </summary>
        /// <returns>True if you want the save command to close the window.</returns>
        protected virtual bool OnSaveCommand()
        {
            OnPropertyFormClosed(true);
            return true;
        }

        #endregion
        /// <summary>
        /// Override this message to force OnPropertyChanged methods
        /// rather than relying on the Setters to do it all.
        /// </summary>
        public virtual void OnPropertyFormClosed(bool okClicked)
        {
        }

    }
}
