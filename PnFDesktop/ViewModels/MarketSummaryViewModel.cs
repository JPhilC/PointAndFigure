using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Controls;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PnFDesktop.ViewModels
{
    public class MarketSummaryViewModel : PaneViewModel
    {
        public ObservableCollection<string> ExchangeCodes { get => SimpleIoc.Default.GetInstance<MainViewModel>().ExchangeCodes; }

        private string _selectedExchangeCode = "LSE";
        public string SelectedExchangeCode
        {
            get => _selectedExchangeCode;
            set
            {
                if (SetProperty(ref _selectedExchangeCode, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshMarketSummary));
                }
            }
        }

        public ObservableCollection<DayDTO> Days { get; } = new ObservableCollection<DayDTO>();


        private DayDTO _selectedDay;

        public DayDTO SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshMarketSummary));
                }
            }
        }

        public ObservableCollection<MarketSummaryDTO> Indices { get; } = new ObservableCollection<MarketSummaryDTO>();

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
        public MarketSummaryViewModel()
        {
            ContentId = Constants.MarketSummary;
            Title = "Market Summary";
            BindingOperations.EnableCollectionSynchronization(Days, _ItemsLock);
            BindingOperations.EnableCollectionSynchronization(Indices, _ItemsLock);

        }

        [PreferredConstructor]
        public MarketSummaryViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
            {
                if (message.Notification == Constants.MarketSummaryUILoaded && !_dataLoaded)
                {
                    await LoadMarketSummaryDataAsync();
                }
                else if (message.Notification == Constants.RefreshMarketSummary && _dataLoaded)
                {
                    await RefreshMarketSummaryDataAsync();
                }
            });

            Control = new MarketSummaryView(this);
        }

        private async Task LoadMarketSummaryDataAsync()
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = true);
                var dates = await _DataService!.GetMarketAvailableDates(DateTime.Now.AddDays(-60));
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Days.Clear());
                    foreach (DayDTO day in dates.OrderByDescending(d => d.Day))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Days.Add(day);
                        });
                    }
                }

                if (this.Days.Any())
                {
                    DayDTO latestDay = dates.LastOrDefault();
                    var list = await _DataService.GetMarketValuesAsync(latestDay!.Day, this.SelectedExchangeCode);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Indices.Clear());
                        foreach (MarketSummaryDTO index in list)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Indices.Add(index);
                            });
                        }
                        App.Current.Dispatcher.Invoke(() => SelectedDay = latestDay);
                    }
                }
                else
                {
                    MessageLog.LogMessage(this, LogType.Information, "There is no market value data available");
                }
                _dataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the market value data", ex);
            }
            finally
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = false);
            }
        }

        private async Task RefreshMarketSummaryDataAsync()
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = true);
                var list = await _DataService!.GetMarketValuesAsync(this.SelectedDay!.Day, this.SelectedExchangeCode);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Indices.Clear());
                    foreach (MarketSummaryDTO index in list)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Indices.Add(index);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the market value data", ex);
            }
            finally
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = false);
            }
        }


        #region LoadSharesCommand ...
        private RelayCommand<MarketSummaryDTO> _loadSharesCommand;

        public RelayCommand<MarketSummaryDTO> LoadSharesCommand
        {
            get
            {
                return _loadSharesCommand ?? (_loadSharesCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage<MarketSummaryDTO>>(
                        new NotificationMessage<MarketSummaryDTO>(this, currentSector, Constants.OpenSharesSummaryPage)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadIndexChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadIndexChartCommand
        {
            get
            {
                return _loadIndexChartCommand ?? (_loadIndexChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.Index)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadIndexRSChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadIndexRSChartCommand
        {
            get
            {
                return _loadIndexRSChartCommand ?? (_loadIndexRSChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.RSSectorVMarket)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadBullishPercentChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadBullishPercentChartCommand
        {
            get
            {
                return _loadBullishPercentChartCommand ?? (_loadBullishPercentChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexBullishPercent)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadPercentAbove10EmaChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadPercentAbove10EmaChartCommand
        {
            get
            {
                return _loadPercentAbove10EmaChartCommand ?? (_loadPercentAbove10EmaChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexPercentShareAbove10)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadPercentAbove30EmaChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadPercentAbove30EmaChartCommand
        {
            get
            {
                return _loadPercentAbove30EmaChartCommand ?? (_loadPercentAbove30EmaChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexPercentShareAbove30)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadPercentRSBuyChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadPercentRSBuyChartCommand
        {
            get
            {
                return _loadPercentRSBuyChartCommand ?? (_loadPercentRSBuyChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexPercentShareRsBuy)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadPercentRSRisingChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadPercentRSRisingChartCommand
        {
            get
            {
                return _loadPercentRSRisingChartCommand ?? (_loadPercentRSRisingChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexPercentShareRsX)
                        );
                }));
            }
        }

        private RelayCommand<MarketSummaryDTO> _loadPercentPTChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadPercentPTChartCommand
        {
            get
            {
                return _loadPercentPTChartCommand ?? (_loadPercentPTChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.IndexPercentSharePT)
                        );
                }));
            }
        }
        #endregion
    }
}
