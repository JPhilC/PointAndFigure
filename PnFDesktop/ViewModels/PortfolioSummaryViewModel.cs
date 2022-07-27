using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Controls;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PnFDesktop.ViewModels
{
    public class PortfolioSummaryViewModel : PaneViewModel
    {

        private Portfolio _portfolio;
        public Portfolio Portfolio
        {
            get => _portfolio;
            set
            {
                if (SetProperty(ref _portfolio, value))
                {
                    ContentId = $"{Constants.PortfolioSummary}_{_portfolio.Id}";
                    Title = $"{_portfolio.Name} Summary" ?? "Summary";
                }
            }
        }

        public ObservableCollection<DayDTO> Days { get => SimpleIoc.Default.GetInstance<MainViewModel>().AvailableDays; }


        private DayDTO? _selectedDay;

        public DayDTO? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshPortfolioSummary));
                }
            }
        }

        private DateTime _customDay;

        public DateTime CustomDay
        {
            get => _customDay;
            set
            {
                if (SetProperty(ref _customDay, value))
                {
                    DayDTO selectedDay = Days.FirstOrDefault(d => d.Day == _customDay.Date);
                    if (selectedDay == null)
                    {
                        selectedDay = new DayDTO() { Day = _customDay.Date };
                        SimpleIoc.Default.GetInstance<MainViewModel>().AvailableDays.Add(selectedDay);
                    }
                    SelectedDay = selectedDay;
                }
            }
        }


        public ObservableCollection<PortfolioShareSummaryDTO> Shares { get; } = new ObservableCollection<PortfolioShareSummaryDTO>();

        readonly IDataService? _DataService;
        private readonly object _ItemsLock = new object();

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

        public PortfolioSummaryViewModel()
        {
            ContentId = Constants.PortfolioSummary;
            Title = "Portfolio Summary";
            BindingOperations.EnableCollectionSynchronization(Days, _ItemsLock);
            BindingOperations.EnableCollectionSynchronization(Shares, _ItemsLock);
        }

        [PreferredConstructor]
        public PortfolioSummaryViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
            {
                if (message.Notification == Constants.PortfolioSummaryUILoaded && !_dataLoaded)
                {
                    this._selectedDay = this.Days.FirstOrDefault();
                    this.CustomDay = this._selectedDay.Day;
                    await LoadPortfolioSharesSummaryDataAsync();
                }
                else if (message.Notification == Constants.RefreshPortfolioSummary && _dataLoaded)
                {
                    await RefreshPortfolioSharesSummaryDataAsync();
                }
            });

            Control = new PortfolioSummaryView(this);
        }

        private async Task LoadPortfolioSharesSummaryDataAsync()
        {
            try
            {
                if (this.Days.Any())
                {
                    var list = await _DataService!.GetPortfolioValuesAsync(this.Portfolio, this.SelectedDay!.Day);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Shares.Clear());
                        foreach (PortfolioShareSummaryDTO share in list.OrderBy(s => s.Tidm))
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Shares.Add(share);
                            });
                        }
                    }
                }
                else
                {
                    MessageLog.LogMessage(this, LogType.Information, "There is no share summary data available");
                }
                App.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(SelectedDay)));
                _dataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share summary data", ex);
            }
        }

        private async Task RefreshPortfolioSharesSummaryDataAsync()
        {
            try
            {
                var list = await _DataService!.GetPortfolioValuesAsync(this.Portfolio, this.SelectedDay!.Day);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Shares.Clear());
                    foreach (PortfolioShareSummaryDTO share in list)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Shares.Add(share);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share summary data", ex);
            }
        }

        #region Relay commands ...

        #region Share related commands ...
        private RelayCommand<PortfolioShareSummaryDTO> _loadShareChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadShareChartCommand
        {
            get
            {
                return _loadShareChartCommand ?? (_loadShareChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.Share, currentShare.Tidm, currentShare.Name)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSharePeerRsChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSharePeerRsChartCommand
        {
            get
            {
                return _loadSharePeerRsChartCommand ?? (_loadSharePeerRsChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.RSStockVSector)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadShareMarketRsChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadShareMarketRsChartCommand
        {
            get
            {
                return _loadShareMarketRsChartCommand ?? (_loadShareMarketRsChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.RSStockVMarket)
                        );
                }));
            }
        }

        #endregion

        #region Market related relay commands ...

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketIndexChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketIndexChartCommand
        {
            get
            {
                return _loadMarketIndexChartCommand ?? (_loadMarketIndexChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.Index)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketIndexRSChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketIndexRSChartCommand
        {
            get
            {
                return _loadMarketIndexRSChartCommand ?? (_loadMarketIndexRSChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.RSSectorVMarket)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketBullishPercentChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketBullishPercentChartCommand
        {
            get
            {
                return _loadMarketBullishPercentChartCommand ?? (_loadMarketBullishPercentChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexBullishPercent)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketPercentAbove10EmaChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketPercentAbove10EmaChartCommand
        {
            get
            {
                return _loadMarketPercentAbove10EmaChartCommand ?? (_loadMarketPercentAbove10EmaChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexPercentShareAbove10)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketPercentAbove30EmaChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketPercentAbove30EmaChartCommand
        {
            get
            {
                return _loadMarketPercentAbove30EmaChartCommand ?? (_loadMarketPercentAbove30EmaChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexPercentShareAbove30)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketHighLowIndexChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketHighLowIndexChartCommand
        {
            get
            {
                return _loadMarketHighLowIndexChartCommand ?? (_loadMarketHighLowIndexChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.HighLowIndex)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketPercentRSBuyChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketPercentRSBuyChartCommand
        {
            get
            {
                return _loadMarketPercentRSBuyChartCommand ?? (_loadMarketPercentRSBuyChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexPercentShareRsBuy)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketPercentRSRisingChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketPercentRSRisingChartCommand
        {
            get
            {
                return _loadMarketPercentRSRisingChartCommand ?? (_loadMarketPercentRSRisingChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexPercentShareRsX)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadMarketPercentPTChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadMarketPercentPTChartCommand
        {
            get
            {
                return _loadMarketPercentPTChartCommand ?? (_loadMarketPercentPTChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.MarketIndexId, PnFData.Model.PnFChartSource.IndexPercentSharePT)
                        );
                }));
            }
        }
        #endregion

        #region Sector related relay commands ...

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorIndexChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorIndexChartCommand
        {
            get
            {
                return _loadSectorIndexChartCommand ?? (_loadSectorIndexChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.Index)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorIndexRSChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorIndexRSChartCommand
        {
            get
            {
                return _loadSectorIndexRSChartCommand ?? (_loadSectorIndexRSChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.RSSectorVMarket)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorBullishPercentChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorBullishPercentChartCommand
        {
            get
            {
                return _loadSectorBullishPercentChartCommand ?? (_loadSectorBullishPercentChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexBullishPercent)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorPercentAbove10EmaChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorPercentAbove10EmaChartCommand
        {
            get
            {
                return _loadSectorPercentAbove10EmaChartCommand ?? (_loadSectorPercentAbove10EmaChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexPercentShareAbove10)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorPercentAbove30EmaChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorPercentAbove30EmaChartCommand
        {
            get
            {
                return _loadSectorPercentAbove30EmaChartCommand ?? (_loadSectorPercentAbove30EmaChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexPercentShareAbove30)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorHighLowIndexChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorHighLowIndexChartCommand
        {
            get
            {
                return _loadSectorHighLowIndexChartCommand ?? (_loadSectorHighLowIndexChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.HighLowIndex)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorPercentRSBuyChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorPercentRSBuyChartCommand
        {
            get
            {
                return _loadSectorPercentRSBuyChartCommand ?? (_loadSectorPercentRSBuyChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexPercentShareRsBuy)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorPercentRSRisingChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorPercentRSRisingChartCommand
        {
            get
            {
                return _loadSectorPercentRSRisingChartCommand ?? (_loadSectorPercentRSRisingChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexPercentShareRsX)
                        );
                }));
            }
        }

        private RelayCommand<PortfolioShareSummaryDTO> _loadSectorPercentPTChartCommand;

        public RelayCommand<PortfolioShareSummaryDTO> LoadSectorPercentPTChartCommand
        {
            get
            {
                return _loadSectorPercentPTChartCommand ?? (_loadSectorPercentPTChartCommand = new RelayCommand<PortfolioShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.SectorIndexId, PnFData.Model.PnFChartSource.IndexPercentSharePT)
                        );
                }));
            }
        }
        #endregion

        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand(async () =>
                {
                    await RefreshPortfolioSharesSummaryDataAsync();
                }));
            }
        }


        #endregion


    }
}
