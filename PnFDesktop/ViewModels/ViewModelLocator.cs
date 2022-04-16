using CommonServiceLocator;
using PnFData.Model;
using PnFDesktop.Classes;
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
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

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
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }


        public MessagesViewModel MessagesViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MessagesViewModel>();
            }
        }




        /// <summary>
        /// This is just used to support design mode in the Procedure designer.
        /// </summary>
        public PointAndFigureChartViewModel PointAndFigureChartViewModel
        {
            get
            {
                return new PointAndFigureChartViewModel();
            }
        }

        /// <summary>
        /// This is just used to support design mode in the case designer.
        /// </summary>
        public PointAndFigureChartViewModel GetPointAndFigureChartViewModel(PnFChart newChart, bool forceRefresh = false)
        {
            string key = "PointAndFigureChart_" + newChart.Name;
            PointAndFigureChartViewModel vm = null;
            if (forceRefresh && SimpleIoc.Default.IsRegistered<PointAndFigureChartViewModel>(key))
            {
                // If the viewmodel is already registered get it.
                SimpleIoc.Default.Unregister<PointAndFigureChartViewModel>(key);
            }

            if (SimpleIoc.Default.IsRegistered<PointAndFigureChartViewModel>(key))
            {
                // If the viewmodel is already registered get it.
                vm = ServiceLocator.Current.GetInstance<PointAndFigureChartViewModel>(key);
            }
            else
            {
                // Otherwise create it and register it before returning
                vm = new PointAndFigureChartViewModel(newChart);
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
                return ServiceLocator.Current.GetInstance<PageLayoutViewModel>();
            }
        }


        /// <summary>
        /// Viewmodel for design binding to ProcedureViewModel
        /// </summary>
        public PointAndFigureChartViewModel ProcedureViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PointAndFigureChartViewModel>();
            }
        }

        /// <summary>
        /// Viewmodel for managing batches
        /// </summary>
        public UserOptionsViewModel UserOptionsViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UserOptionsViewModel>();
            }
        }

        /// <summary>
        /// Viewmodel for Splash Screen
        /// </summary>
        public SplashScreenViewModel SplashScreenViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SplashScreenViewModel>();
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
