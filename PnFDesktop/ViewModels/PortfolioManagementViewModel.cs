using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Controls;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PnFDesktop.ViewModels
{
    public class PortfolioManagementViewModel : PaneViewModel
    {
        private Portfolio _portfolio;

        public Portfolio Portfolio
        {
            get => _portfolio;
            set
            {
                if (SetProperty(ref _portfolio, value))
                {
                    LoadPortfolioShares();
                    ContentId = $"{Constants.PortfolioManagement}_{_portfolio.Id}";
                    Title = _portfolio.Name ?? "Portfolio";
                    OnPropertyChanged("Name");
                }
            }
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
                SavePortfolio();
            }
        }

        private string _shareTidm = "";

        public string ShareTidm
        {
            get => _shareTidm;
            set
            {
                if (SetProperty(ref _shareTidm, value))
                {
                    SetSelectedShare();
                }
            }
        }

        private void SetSelectedShare()
        {
            Task.Run(async () =>
            {
                Share share = await _DataService.GetShareAsync(_shareTidm);
                await DispatcherHelper.RunAsync(() =>
                {
                    if (share != null)
                    {
                        SelectedShare = share;
                    }
                    else
                    {
                        SelectedShare = null;
                        MessageLog.LogMessage(this, LogType.Information, $"Tidm {ShareTidm} is not recognised.");
                    }
                    AddSelectedShareCommand.NotifyCanExecuteChanged();
                });
            });
        }

        private Share? _selectedShare;
        public Share? SelectedShare
        {
            get => _selectedShare;
            set => SetProperty(ref _selectedShare, value);
        }


        private double _holdingQty = 0d;

        public double HoldingQty
        {
            get => _holdingQty;
            set => SetProperty(ref _holdingQty, value);
        }

        public ObservableCollection<PortfolioShareDTO> PortfolioShares { get; } = new ObservableCollection<PortfolioShareDTO>();

        private PortfolioShareDTO? _selectedPortfolioShare;
        public PortfolioShareDTO? SelectedPortfolioShare
        {
            get => _selectedPortfolioShare;
            set
            {
                if (SetProperty(ref _selectedPortfolioShare, value))
                {
                    DeletePorfolioShareCommand.NotifyCanExecuteChanged();
                }
            }
        }


        IDataService _DataService;
        private object _SharesLock = new object();
        private object _PortLock = new object();

        #region Control property ...

        private UserControl _control = null;

        /// <summary>
        /// Sets and gets the Control property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public UserControl Control
        {
            get => _control;
            set => SetProperty(ref _control, value);
        }
        #endregion

        private bool _dataLoaded = false;


        public PortfolioManagementViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(PortfolioShares, _PortLock);
        }

        [PreferredConstructor]
        public PortfolioManagementViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
            {
                if (message.Notification == Constants.PortfolioManagementUILoaded && !_dataLoaded)
                {
                    // await LoadSharesAsync();
                }
            });

            Control = new PortfolioManagementView(this);
        }

        private void LoadPortfolioShares()
        {
            PortfolioShares.Clear();
            if (this.Portfolio != null)
            {
                foreach (PortfolioShare share in this.Portfolio.Shares)
                {
                    PortfolioShares.Add(new PortfolioShareDTO()
                    {
                        Id = share.Id,
                        Holding = share.Holding,
                        Tidm = share.Share.Tidm,
                        Name = share.Share.Name
                    });
                    ;
                }
            }
        }

        private void SavePortfolio()
        {
            Task.Run(async () => await _DataService.UpdatePortfolioAsync(this.Portfolio));
        }

        #region Relay Commands ...
        private RelayCommand _addSelectedShareCommand;

        public RelayCommand AddSelectedShareCommand
        {
            get
            {
                return _addSelectedShareCommand
                       ?? (_addSelectedShareCommand = new RelayCommand(
                           async () =>
                           {
                               if (this.SelectedShare != null)
                               {
                                   this.Portfolio.Shares.Add(new PortfolioShare()
                                   {
                                       Holding = HoldingQty,
                                       ShareId = SelectedShare.Id,
                                       Share = SelectedShare
                                   });
                                   await _DataService.UpdatePortfolioAsync(this.Portfolio);
                                   LoadPortfolioShares();
                               }
                           },
                           () => SelectedShare != null));
            }
        }



        private RelayCommand<PortfolioShareDTO> _deletePorfolioShareCommand;

        public RelayCommand<PortfolioShareDTO> DeletePorfolioShareCommand
        {
            get
            {
                return _deletePorfolioShareCommand
                       ?? (_deletePorfolioShareCommand = new RelayCommand<PortfolioShareDTO>(
                           async (shareDTO) =>
                           {
                               if (shareDTO != null)
                               {
                                   PortfolioShare share = this.Portfolio.Shares.FirstOrDefault(s => s.Id == shareDTO.Id);
                                   if (share != null)
                                   {
                                       if (await _DataService.DeletePortfolioShareAsync(share))
                                       {
                                           LoadPortfolioShares();
                                       }
                                   }
                               }
                           },
                           (shareDTO) => SelectedPortfolioShare != null));
            }
        }
        #endregion
    }
}
