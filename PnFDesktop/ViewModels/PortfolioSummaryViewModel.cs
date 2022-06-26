using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
using PnFDesktop.Classes;
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
    public class PortfolioSummaryViewModel:PaneViewModel
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
                    Title = _portfolio.Name ?? " Summary";
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
                    await LoadPortfolioSharesSummaryDataAsync();
                }
            });

            Control = new PortfolioSummaryView(this);
        }

        private async Task LoadPortfolioSharesSummaryDataAsync()
        {
            try
            {
                var list = await _DataService!.GetPortfolioValuesAsync(this.Portfolio, this.SelectedDay!.Day);
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Shares.Clear());
                    foreach (ShareSummaryDTO share in list.OrderBy(s => s.Tidm))
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Shares.Add(share);
                        });
                    }
                    _dataLoaded = true;
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share summary data", ex);
            }
        }



    }
}
