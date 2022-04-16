using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFDesktop.Classes;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PnFDesktop.ViewModels
{
    public enum IndexChartType
    {
        [Description("Index")]
        Index = 1,
        [Description("Sector RS")]                                  //
        RSSectorVMarket = 4,
        [Description("Bullish Percent")]                            // IndexIndicator.BullishPercent
        IndexBullishPercent = 5,
        [Description("Percent of Share RS on Buy")]                 // IndexIndicator.PercentShareRsBuy
        IndexPercentShareRsBuy = 6,
        [Description("Percent of Share RS on X")]                   // IndexIndicator.PercentShareRsRising
        IndexPercentShareRsX = 7,
        [Description("Percent of Shares with Positive Trends")]     // IndexIndicator.PercentSharePt
        IndexPercentSharePT = 8,
        [Description("Percent of Shares above 30 EMA")]             // IndexIndicator.PercentAboveEma30
        IndexPercentShareAbove30 = 9,
        [Description("Percent of Shares bove 10 EMA")]              // IndexIndicator.PercentAboveEma10
        IndexPercentShareAbove10 = 10,
        [Description("High-Low Index")]
        HighLowIndex = 11,
        [Description("Advance-Decline Line")]
        AdvanceDeclineLine = 12
    }

    public class OpenIndexChartViewModel : ViewModelBase
    {
        private IndexChartType _shareChartType;

        public IndexChartType IndexChartType
        {
            get => _shareChartType;
            set => SetProperty(ref _shareChartType, value);
        }

        public ObservableCollection<IndexDTO> Indices { get; } = new ObservableCollection<IndexDTO>();

        private IndexDTO? _selectedIndex;
        public IndexDTO? SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        IDataService _DataService;
        private object _ItemsLock = new object();

        public OpenIndexChartViewModel(IDataService dataService)
        {
            _DataService = dataService;
            BindingOperations.EnableCollectionSynchronization(Indices, _ItemsLock);


            Messenger.Default.Register<NotificationMessage>(this, async message =>
            {
                if (message.Notification == "OpenIndexChartWindowLoaded")
                {
                    await LoadIndicesAsync();
                }
            });
        }

        private async Task LoadIndicesAsync()
        {
            try
            {
                var list = await _DataService.GetIndicesAsync();
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Indices.Clear());
                    foreach (IndexDTO index in list)
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
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the index data", ex);
            }
        }
    }
}
