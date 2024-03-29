﻿using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
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
    public class FilteredSharesSummaryViewModel : PaneViewModel
    {
        private ShareEvents _eventFilter = ShareEvents.AllShareSignals;
        public ShareEvents EventFilter
        {
            get => _eventFilter;
            set
            {
                if (SetProperty(ref _eventFilter, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshFilteredSharesSummary));
                }
            }
        }

        public ObservableCollection<string> ExchangeCodes { get => SimpleIoc.Default.GetInstance<MainViewModel>().ExchangeCodes; }

        private string _selectedExchangeCode = "LSE";
        public string SelectedExchangeCode
        {
            get => _selectedExchangeCode;
            set
            {
                if (SetProperty(ref _selectedExchangeCode, value))
                {
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshFilteredSharesSummary));
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
                    WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.RefreshFilteredSharesSummary));
                }
            }
        }

        public ObservableCollection<ShareSummaryDTO> Shares { get; } = new ObservableCollection<ShareSummaryDTO>();

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

        public FilteredSharesSummaryViewModel()
        {
            ContentId = Constants.FilteredSharesSummary;
            Title = "New Shares Events";
            BindingOperations.EnableCollectionSynchronization(Days, _ItemsLock);
            BindingOperations.EnableCollectionSynchronization(Shares, _ItemsLock);
        }


        [PreferredConstructor]
        public FilteredSharesSummaryViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;

            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
                        {
                            if (message.Notification == Constants.FilteredSharesSummaryUILoaded && !_dataLoaded)
                            {
                                this._selectedDay = this.Days.FirstOrDefault();
                                await LoadSummaryDataAsync();
                            }
                            else if (message.Notification == Constants.RefreshFilteredSharesSummary && _dataLoaded)
                            {
                                await RefreshSharesSummaryDataAsync();
                            }
                        });
            Control = new FilteredSharesSummaryView(this);
        }

        private async Task LoadSummaryDataAsync()
        {
            try
            {
                if (this.Days.Any())
                {
                    var list = await _DataService!.GetEventFilteredSharesAsync(this.EventFilter, this.SelectedDay!.Day, this.SelectedExchangeCode);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Shares.Clear());
                        foreach (ShareSummaryDTO share in list)
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

        private async Task RefreshSharesSummaryDataAsync()
        {
            try
            {
                var list = await _DataService!.GetEventFilteredSharesAsync(this.EventFilter, this.SelectedDay!.Day, this.SelectedExchangeCode);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Shares.Clear());
                    foreach (ShareSummaryDTO share in list)
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
        private RelayCommand<ShareSummaryDTO> _loadShareChartCommand;

        public RelayCommand<ShareSummaryDTO> LoadShareChartCommand
        {
            get
            {
                return _loadShareChartCommand ?? (_loadShareChartCommand = new RelayCommand<ShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.Share, currentShare.Tidm, currentShare.Name)
                        );
                }));
            }
        }

        private RelayCommand<ShareSummaryDTO> _loadSharePeerRsChartCommand;

        public RelayCommand<ShareSummaryDTO> LoadSharePeerRsChartCommand
        {
            get
            {
                return _loadSharePeerRsChartCommand ?? (_loadSharePeerRsChartCommand = new RelayCommand<ShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.RSStockVSector)
                        );
                }));
            }
        }

        private RelayCommand<ShareSummaryDTO> _loadShareMarketRsChartCommand;

        public RelayCommand<ShareSummaryDTO> LoadShareMarketRsChartCommand
        {
            get
            {
                return _loadShareMarketRsChartCommand ?? (_loadShareMarketRsChartCommand = new RelayCommand<ShareSummaryDTO>(currentShare =>
                {
                    WeakReferenceMessenger.Default.Send<OpenPointAndFigureChartMessage>(
                        new OpenPointAndFigureChartMessage(this, currentShare.Id, PnFData.Model.PnFChartSource.RSStockVMarket)
                        );
                }));
            }
        }
        #endregion
    }
}
