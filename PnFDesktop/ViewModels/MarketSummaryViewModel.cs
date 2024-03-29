﻿using Microsoft.Toolkit.Mvvm.Input;
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
using System.Threading;
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

        public ObservableCollection<DayDTO> Days { get => SimpleIoc.Default.GetInstance<MainViewModel>().AvailableDays; }


        private DayDTO? _selectedDay;

        public DayDTO? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshMarketSummary));
                    if (_selectedDay != null)
                    {
                        _customDay = _selectedDay.Day;
                        OnPropertyChanged("CustomDay");
                    }

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

        public ObservableCollection<MarketSummaryDTO> Markets { get; } = new ObservableCollection<MarketSummaryDTO>();

        public ObservableCollection<MarketSummaryDTO> Sectors { get; } = new ObservableCollection<MarketSummaryDTO>();

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
            BindingOperations.EnableCollectionSynchronization(Markets, _ItemsLock);
            BindingOperations.EnableCollectionSynchronization(Sectors, _ItemsLock);

        }

        [PreferredConstructor]
        public MarketSummaryViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
            {
                if (message.Notification == Constants.MarketSummaryUILoaded && !_dataLoaded)
                {
                    this._selectedDay = this.Days.FirstOrDefault();
                    this.CustomDay = this._selectedDay.Day;
                    await LoadMarketSummaryDataAsync();
                    await LoadSectorSummaryDataAsync();
                    MoveMonthCommand.NotifyCanExecuteChanged();
                    MoveWeekCommand.NotifyCanExecuteChanged();

                }
                else if (message.Notification == Constants.RefreshMarketSummary && _dataLoaded)
                {
                    await RefreshMarketSummaryDataAsync();
                    await RefreshSectorSummaryDataAsync();
                }
            });

            Control = new MarketSummaryButtonView(this);
        }

        private async Task LoadMarketSummaryDataAsync()
        {
            try
            {
                if (this.Days.Any())
                {
                    var list = await _DataService.GetMarketValuesAsync(this.SelectedDay!.Day);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Markets.Clear());
                        foreach (MarketSummaryDTO index in list)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Markets.Add(index);
                            });
                        }
                    }
                }
                else
                {
                    MessageLog.LogMessage(this, LogType.Information, "There is no market value data available");
                }
                App.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(SelectedDay)));
                _dataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the market value data", ex);
            }
        }

        private async Task LoadSectorSummaryDataAsync()
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = true);
                if (this.Days.Any())
                {
                    var list = await _DataService.GetSectorValuesAsync(this.SelectedDay!.Day, this.SelectedExchangeCode);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Sectors.Clear());
                        foreach (MarketSummaryDTO index in list)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Sectors.Add(index);
                            });
                        }
                    }
                }
                else
                {
                    MessageLog.LogMessage(this, LogType.Information, "There is no sector value data available");
                }
                App.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(SelectedDay)));
                _dataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the secto value data", ex);
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
                var list = await _DataService!.GetMarketValuesAsync(this.SelectedDay!.Day);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Markets.Clear());
                    foreach (MarketSummaryDTO index in list)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Markets.Add(index);
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

        private async Task RefreshSectorSummaryDataAsync()
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = true);
                var list = await _DataService!.GetSectorValuesAsync(this.SelectedDay!.Day, this.SelectedExchangeCode);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Sectors.Clear());
                    foreach (MarketSummaryDTO index in list)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Sectors.Add(index);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the sector value data", ex);
            }
            finally
            {
                App.Current.Dispatcher.Invoke(() => IsBusy = false);
            }
        }


        #region Relay commands ...
        private RelayCommand<int> _moveMonthCommand;

        /// <summary>
        /// Moves back or forward in multiples of 4 weeks.
        /// </summary>
        public RelayCommand<int> MoveMonthCommand
        {
            get
            {
                return _moveMonthCommand ?? (_moveMonthCommand = new RelayCommand<int>(increment =>
                {
                    CustomDay = CustomDay.AddDays(increment * 28);
                },
                (increment)=>{ return true;}
                ));
            }
        }

        private RelayCommand<int> _moveWeekCommand;

        public RelayCommand<int> MoveWeekCommand
        {
            get
            {
                return _moveWeekCommand ?? (_moveWeekCommand = new RelayCommand<int>(increment =>
                {
                    CustomDay = CustomDay.AddDays(increment * 7);
                }));
            }
        }



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

        private RelayCommand<MarketSummaryDTO> _loadHighLowIndexChartCommand;

        public RelayCommand<MarketSummaryDTO> LoadHighLowIndexChartCommand
        {
            get
            {
                return _loadHighLowIndexChartCommand ?? (_loadHighLowIndexChartCommand = new RelayCommand<MarketSummaryDTO>(currentSector =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentSector.Id, PnFData.Model.PnFChartSource.HighLowIndex)
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
