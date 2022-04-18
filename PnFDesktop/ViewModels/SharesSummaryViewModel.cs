using Microsoft.Toolkit.Mvvm.Input;
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
    public class SharesSummaryViewModel : PaneViewModel
    {
        private MarketSummaryDTO _marketSummaryDTO;
        public MarketSummaryDTO MarketSummaryDTO
        {
            get => _marketSummaryDTO;
            set
            {
                if (SetProperty(ref _marketSummaryDTO, value))
                {
                    ContentId = $"{Constants.SharesSummary}_{_marketSummaryDTO.Id}_{_marketSummaryDTO.Day.ToString("yyyyMMdd")}";
                    Title = _marketSummaryDTO.Description??"Shares Summary";

                }
            }
        }


        private DateTime _selectedDay;

        public DateTime SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
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

        public SharesSummaryViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(Shares, _ItemsLock);

        }

        [PreferredConstructor]
        public SharesSummaryViewModel(IDataService dataService) : this()
        {
            _DataService = dataService;
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this, async (r, message) =>
            {
                if (message.Notification == Constants.SharesSummaryUILoaded)
                {
                    System.Diagnostics.Debug.Assert(MarketSummaryDTO != null, "The MarketSummary property needs setting before attempting to load data");
                    await LoadSharesSummaryDataAsync();
                }
            });

            Control = new SharesSummaryView(this);
        }

        private async Task LoadSharesSummaryDataAsync()
        {
            try
            {
                var list = await _DataService!.GetShareValuesAsync(this.MarketSummaryDTO!);
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
                    App.Current.Dispatcher.Invoke(() => SelectedDay = this.MarketSummaryDTO.Day);
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share summary data", ex);
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
                    System.Diagnostics.Debug.WriteLine($"Signal to load shares for {currentSector.Description}");


                }));

            }
        }

        #endregion
    }
}
