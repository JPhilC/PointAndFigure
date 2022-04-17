using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
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
        public ObservableCollection<DateTime> Days { get; } = new ObservableCollection<DateTime>();


        private DateTime _selectedDay;

        public DateTime SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }

        public ObservableCollection<MarketSummaryValueDTO> Indices { get; } = new ObservableCollection<MarketSummaryValueDTO>();

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
                if (message.Notification == Constants.MarketSummaryUILoaded)
                {
                    await LoadMarketSummaryDataAsync();
                }
            });

            Control = new MarketSummaryView(this);
        }

        private async Task LoadMarketSummaryDataAsync()
        {
            try
            {
                var dates = await _DataService!.GetMarketAvailableDates(DateTime.Now.AddDays(-60));
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Days.Clear());
                    foreach (DateTime day in dates.OrderByDescending(d => d))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Days.Add(day);
                        });
                    }
                }

                DateTime? latestDay = dates.Max(d => d);
                if (latestDay != null)
                {
                    SelectedDay = latestDay.Value;
                    var list = await _DataService.GetMarketValuesAsync(latestDay.Value);
                    lock (_ItemsLock)
                    {
                        App.Current.Dispatcher.Invoke(() => Indices.Clear());
                        foreach (MarketSummaryValueDTO index in list)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Indices.Add(index);
                            });
                        }
                    }

                }
                else
                {
                    MessageLog.LogMessage(this, LogType.Information, "There is no market value data available");
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the market value data", ex);
            }
        }

    }
}
