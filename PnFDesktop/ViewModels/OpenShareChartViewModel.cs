using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFDesktop.Classes;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
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
    public enum ShareChartType
    {
        [Description("Share")]
        Share = 0,
        [Description("Share RS")]
        RSStockVMarket = 2,
        [Description("Peer RS")]
        RSStockVSector = 3
    }

    public class OpenShareChartViewModel : ViewModelBase
    {
        private string _shareTidm = "";

        public string ShareTidm
        {
            get => _shareTidm;
            set
            {
                if (SetProperty(ref _shareTidm, value))
                {
                    SelectedShare = Shares.FirstOrDefault(s=> s.Tidm == value);
                }
            }
        }


        private ShareChartType _shareChartType;

        public ShareChartType ShareChartType
        {
            get => _shareChartType;
            set => SetProperty(ref _shareChartType, value);
        }

        public ObservableCollection<ShareDTO> Shares { get; } = new ObservableCollection<ShareDTO>();

        private ShareDTO? _selectedShare;
        public ShareDTO? SelectedShare
        {
            get => _selectedShare;
            set => SetProperty(ref _selectedShare, value);
        }

        IDataService _DataService;
        private object _ItemsLock = new object();

        public OpenShareChartViewModel(IDataService dataService)
        {
            _DataService = dataService;
            BindingOperations.EnableCollectionSynchronization(Shares, _ItemsLock);

            Task.Run(() => LoadShares());
        }

        private async Task LoadShares()
        {
            try
            {
                var list = await _DataService.GetSharesAsync();
                lock (_ItemsLock)
                {
                    App.Current.Dispatcher.Invoke(() => Shares.Clear());
                    foreach (ShareDTO share in list)
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
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share data", ex);
            }
        }
    }
}
