using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Services;
using PnFDesktop.ViewCharts;
using System.Windows;

namespace PnFDesktop.ViewModels
{
    public class ViewModelLocator
    {
        private static ViewModelLocator _current = null;

        public static ViewModelLocator Current
        {
            get
            {
                if (_current == null)
                {
                    // Get a reference to the App Resource version
                    _current = (ViewModelLocator)Application.Current.Resources["Locator"];
                }
                return _current;
            }
        }


        
        public ViewModelLocator() { }

        static ViewModelLocator()
        {
            if (DesignerLibrary.IsInDesignMode)
            {
                SimpleIoc.Default.Register<IDataService, DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MessagesViewModel>();
            SimpleIoc.Default.Register<PageLayoutViewModel>();
            SimpleIoc.Default.Register<PointAndFigureChartViewModel>();
            SimpleIoc.Default.Register<UserOptionsViewModel>();
            SimpleIoc.Default.Register<SplashScreenViewModel>();
            SimpleIoc.Default.Register<OpenShareChartViewModel>();
            SimpleIoc.Default.Register<OpenIndexChartViewModel>();
            SimpleIoc.Default.Register<MarketSummaryViewModel>();
            SimpleIoc.Default.Register<SharesSummaryViewModel>();
        }



        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel MainViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MainViewModel>();
            }
        }


        public MessagesViewModel MessagesViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MessagesViewModel>();
            }
        }




        /// <summary>
        /// This is just used to support design mode in the Procedure designer.
        /// </summary>
        public PointAndFigureChartViewModel PointAndFigureChartViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<PointAndFigureChartViewModel>();
            }
        }

        /// <summary>
        /// This is just used to support design mode in the case designer.
        /// </summary>
        public PointAndFigureChartViewModel GetPointAndFigureChartViewModel(PnFChart newChart, bool forceRefresh = false)
        {
            string key = $"{Constants.PointAndFigureChart}_{newChart.Id}";
            PointAndFigureChartViewModel vm = null;
            if (forceRefresh && SimpleIoc.Default.IsRegistered<PointAndFigureChartViewModel>(key))
            {
                // If the viewmodel is already registered get it.
                SimpleIoc.Default.Unregister<PointAndFigureChartViewModel>(key);
            }

            if (SimpleIoc.Default.IsRegistered<PointAndFigureChartViewModel>(key))
            {
                // If the viewmodel is already registered get it.
                vm = SimpleIoc.Default.GetInstance<PointAndFigureChartViewModel>(key);
            }
            else
            {
                // Otherwise create it and register it before returning
                vm = new PointAndFigureChartViewModel(newChart);
                SimpleIoc.Default.Register(() => vm, key, true);
            }
            return vm;
        }

        
        public static SharesSummaryViewModel GetSharesSummaryViewModel(MarketSummaryDTO marketSummaryDTO)
        {
            string key = $"{Constants.SharesSummary}_{marketSummaryDTO.Id}_{marketSummaryDTO.Day.ToString("yyyyMMdd")}";
            SharesSummaryViewModel vm = null;

            if (SimpleIoc.Default.IsRegistered<SharesSummaryViewModel>(key))
            {
                // If the viewmodel is already registered get it.
                vm = SimpleIoc.Default.GetInstance<SharesSummaryViewModel>(key);
            }
            else
            {
                // Otherwise create it and register it before returning
                vm = new SharesSummaryViewModel(SimpleIoc.Default.GetInstance<IDataService>());
                vm.MarketSummaryDTO = marketSummaryDTO;
                SimpleIoc.Default.Register(() => vm, key, true);
            }
            return vm;
        }


        /// <summary>
        /// Viewmodel for Page Layout window.
        /// </summary>
        public PageLayoutViewModel PageLayoutViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<PageLayoutViewModel>();
            }
        }



        /// <summary>
        /// Viewmodel for managing batches
        /// </summary>
        public UserOptionsViewModel UserOptionsViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<UserOptionsViewModel>();
            }
        }

        /// <summary>
        /// Viewmodel for Splash Screen
        /// </summary>
        public SplashScreenViewModel SplashScreenViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SplashScreenViewModel>();
            }
        }

        /// <summary>
        /// Viewmodel for the Market Summary page
        /// </summary>
        public MarketSummaryViewModel MarketSummaryViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<MarketSummaryViewModel>();
            }
        }

                /// <summary>
        /// Viewmodel for the Shares Summary page
        /// </summary>
        public SharesSummaryViewModel SharesSummaryViewModel
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SharesSummaryViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }






    }
}
