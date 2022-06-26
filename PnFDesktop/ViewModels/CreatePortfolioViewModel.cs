using Microsoft.Toolkit.Mvvm.Input;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;
using System;
using System.Threading.Tasks;

namespace PnFDesktop.ViewModels
{
    public class CreatePortfolioViewModel : ViewModelBase
    {
        private Portfolio? _portfolio = new Portfolio();

        public Portfolio? Portfolio
        {
            get => _portfolio;
            private set => SetProperty(ref _portfolio, value);
        }

        public string Name
        {
            get => _portfolio != null ? _portfolio.Name : "";
            set
            {
                if (_portfolio == null || _portfolio.Name == value)
                {
                    return;
                }
                _portfolio.Name = value;
                SaveChangesAndCloseCommand.NotifyCanExecuteChanged();
            }
        }

        IDataService _DataService;
        private object _ItemsLock = new object();

        public CreatePortfolioViewModel(IDataService dataService)
        {
            _DataService = dataService;
        }

        private RelayCommand? _saveChangesAndCloseCommand;

        /// <summary>
        /// Gets the SaveChangesAndCloseCommand.
        /// </summary>
        public new RelayCommand SaveChangesAndCloseCommand
        {
            get
            {
                return _saveChangesAndCloseCommand
                    ?? (_saveChangesAndCloseCommand = new RelayCommand(
                                          async () =>
                                          {
                                              if (await OnSaveCommand())
                                              {
                                                  // Don't need to do anything here as the assumption is that the properties
                                                  // and bound and therefore already saved.
                                                  if (this.SaveAndCloseAction != null)
                                                  {
                                                      SaveAndCloseAction();
                                                  }
                                              }
                                          },
                                          () => _portfolio != null && !string.IsNullOrEmpty(_portfolio.Name)));
            }
        }


        protected new async Task<bool> OnSaveCommand()
        {
            // Try saving the new portfolio
            if (await _DataService.UpdatePortfolioAsync(_portfolio!))
            {
                return base.OnSaveCommand();
            }
            else
            {
                return false;
            }
        }
    }
}
