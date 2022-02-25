using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Controls;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using PnFDesktop.ViewCharts;

namespace PnFDesktop.ViewModels
{
    public class MainViewModel: ObservableObject, ILayoutViewModelParent, IViewModelResolver
    {
        public Action ExitApplicationAction { get; set; }

        private readonly IDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the WorkspaceViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            if (!DesignerLibrary.IsInDesignMode)
            {
                _adLayout = new AvalonDockLayoutViewModel(this);
            }

            WeakReferenceMessenger.Default.Register<PointAndFigureChartClosedMessage>(this, (r, message) =>
            {
                if (message.Sender != this)
                {
                    // Remove the ModelDesignerViewpnfChart from the list of open documents.
                    PaneViewModel vm = ChartPanes.FirstOrDefault(m => m.ContentId == "PointAndFigureChart_" + message.ViewModel.Chart.Id.ToString());
                    if (vm != null)
                    {
                        ChartPanes.Remove(vm);
                    }
                }
            });


            WeakReferenceMessenger.Default.Register<ActivePointAndFigureChartChangedMessage>(this, (r, message) =>
            {
                if (message.Sender != this)
                {
                    PointAndFigureChartViewModel pnfChartVm = ChartPanes.FirstOrDefault(c => c is PointAndFigureChartViewModel && ((PointAndFigureChartViewModel)c).Chart.Id == message.Chart.Id) as PointAndFigureChartViewModel;
                    if (pnfChartVm != null)
                    {
                        ActiveDocument = (PaneViewModel)pnfChartVm;
                        ActiveChart = pnfChartVm.Chart;
                        ActiveObject = ActiveChart;
                    }
                }
            });


        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ShutdownService.RequestShutdown();
            e.Cancel = true;
        }

        private bool _isBusy = true;
        /// <summary>
        /// Get/set property to signal whether application is busy or not.
        /// 
        /// The property set method invokes a CommandManager.InvalidateRequerySuggested()
        /// if the new value is different from the old one. The mouse cursor is set to Wait
        /// if the application is busy.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;

            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();

                    CommandManager.InvalidateRequerySuggested();

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (value == false)
                                Mouse.OverrideCursor = null;
                            else
                                Mouse.OverrideCursor = Cursors.Wait;
                        }),
                        DispatcherPriority.Background);
                }
            }
        }

        List<ToolViewModel> _tools = null;
        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                if (_tools == null)
                {
                    LoadTools();
                }
                return _tools;
            }
        }

        private MessagesViewModel _messagePaneVm = null;
        public MessagesViewModel MessagePaneVm
        {
            get
            {
                if (_messagePaneVm == null)
                {
                    _messagePaneVm = ViewModelLocator.Current.MessagesViewModel;
                }
                return _messagePaneVm;
            }
        }

        private AvalonDockLayoutViewModel _adLayout = null;

        public AvalonDockLayoutViewModel AdLayout => _adLayout;

        #region PnFChartPanes property ...
        ObservableCollection<PaneViewModel> _chartPanes;
        public ObservableCollection<PaneViewModel> ChartPanes
        {
            get
            {
                if (_chartPanes == null)
                {
                    _chartPanes = new ObservableCollection<PaneViewModel>();
                }

                return _chartPanes;
            }
        }
        #endregion

        private PaneViewModel _activeDocument = null;


        public PaneViewModel ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (_activeDocument == value)
                {
                    return;
                }
                _activeDocument = value;
                OnPropertyChanged();
                UpdateActiveModel(_activeDocument);
            }
        }


        private void UpdateActiveModel(PaneViewModel paneVm)
        {
            PointAndFigureChartViewModel designerVm = paneVm as PointAndFigureChartViewModel;
            if (designerVm != null)
            {
                ActiveChart = designerVm.Chart;
                ActiveObject = ActiveChart;
            }
        }

        private PnFChart _activeChart = null;

        /// <summary>
        /// Sets and gets the ActiveModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFChart ActiveChart
        {
            get => _activeChart;
            set
            {
                if (SetProperty(ref _activeChart, value))
                {
                    PrintPointAndFigureChartCommand.NotifyCanExecuteChanged();
                }
            }
        }


        private ObservableObject _activeObject = null;

        /// <summary>
        /// Sets and gets the ActiveModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableObject ActiveObject
        {
            get => _activeObject;

            set
            {
                if (_activeObject == value)
                {
                    return;
                }

                _activeObject = value;
                OnPropertyChanged();
            }
        }

        private void LoadTools()
        {
            _tools = new List<ToolViewModel>();
            _tools.Add(MessagePaneVm);
        }

        public void OpenPointAndFigureChart(PnFChart pnfChart, bool makeActive = false, bool forceRefresh = false)
        {
            // Get the ModelDesignerViewModel from the ViewModel locator instance. This is the definitive
            // source for viewpnfCharts.
            PointAndFigureChartViewModel pnfChartDesignerViewModel = ViewModelLocator.Current.GetPointAndFigureChartViewModel(pnfChart, forceRefresh);
            PaneViewModel paneViewModel = pnfChartDesignerViewModel as PaneViewModel;
            if (!ChartPanes.Contains(paneViewModel))
            {
                ChartPanes.Add(paneViewModel);
            }
            if (makeActive)
            {
                ActiveDocument = paneViewModel;
            }
        }

        public void ClosePointAndFigureChart(PnFChart pnfChart)
        {
            // Get the ModelDesignerViewModel from the ViewModel locator instance. This is the definitive
            // source for viewpnfCharts.
            PaneViewModel paneVm = ViewModelLocator.Current.GetPointAndFigureChartViewModel(pnfChart) as PaneViewModel;
            ChartPanes.Remove(paneVm);
        }

        private RelayCommand _userOptionsCommand;

        /// <summary>
        /// Gets the ShowPropertiesCommand.
        /// </summary>
        public RelayCommand UserOptionsCommand
        {
            get
            {
                return _userOptionsCommand
                       ?? (_userOptionsCommand = new RelayCommand(
                           () =>
                           {
                               UserOptionsWindow dialog = new UserOptionsWindow();
                               dialog.Owner = App.Current.MainWindow;
                               dialog.ShowDialog();
                           }));
            }
        }

        private RelayCommand<PointAndFigureChartViewModel> _closePointAndFigureChartCommand;

        /// <summary>
        /// Close the current pnfChart display window
        /// </summary>
        public RelayCommand<PointAndFigureChartViewModel> ClosePointAndFigureChartCommand
        {
            get
            {
                return _closePointAndFigureChartCommand
                       ?? (_closePointAndFigureChartCommand = new RelayCommand<PointAndFigureChartViewModel>(
                           (c) => { ChartPanes.Remove((PaneViewModel)c); }));
            }
        }





        private RelayCommand _printPointAndFigureChartCommand;

        /// <summary>
        /// Print the active case
        /// </summary>
        public RelayCommand PrintPointAndFigureChartCommand
        {
            get
            {
                return _printPointAndFigureChartCommand
                       ?? (_printPointAndFigureChartCommand = new RelayCommand(
                           () => {
                               // Pass on the message to whoever is listening and ready to do the printing
                               // most probably the CaseDesigner control since this has a handle on the visual.
                               WeakReferenceMessenger.Default.Send<PrintPointAndFigureChartMessage>(new PrintPointAndFigureChartMessage(this, ActiveChart.Id));
                           }, () => {
                               return (this.ActiveChart != null);
                           }));
            }
        }


        private RelayCommand _exitCommand;

        /// <summary>
        /// Gets the SaveChangesAndCloseCommand.
        /// </summary>
        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (this.ExitApplicationAction != null)
                                              {
                                                  ExitApplicationAction();
                                              }
                                          }));
            }
        }

        ObservableObject IViewModelResolver.ContentViewModelFromID(string contentId)
        {
            System.Diagnostics.Debug.WriteLine($"Resolving for content id:\"{contentId}\"");
            var anchorable_vm = this.Tools.FirstOrDefault(d => d.ContentId == contentId);
            if (anchorable_vm != null) {
                return anchorable_vm;
            }

            var pnfChartVM = this.ChartPanes.FirstOrDefault(d => d.ContentId == contentId);
            if (pnfChartVM != null)
            {
                return pnfChartVM;
            }


            return this.OpenContentViewModel(contentId);
        }

        public ObservableObject OpenContentViewModel(string contentId)
        {
            System.Diagnostics.Debug.WriteLine($"Opening for content id:\"{contentId}\"");
            /*
            Resolving for content id:"ModelDesigner_a84db061-1a2c-494f-945b-7a5abf7cd8ca"
            Opening for content id:"ModelDesigner_a84db061-1a2c-494f-945b-7a5abf7cd8ca"
            Resolving for content id:"Chart_8a3a28ea-8d5e-40a4-849a-1727ac8a84e1"
            Opening for content id:"Chart_8a3a28ea-8d5e-40a4-849a-1727ac8a84e1"
            */
            string[] splitId = contentId.Split('_');
            if (splitId.Length == 2)
            {
                Guid objectId = new Guid(splitId[1]);
                if (splitId[0] == "PointAndFigureChart")
                {
                    PnFChart chart = null;
                    WeakReferenceMessenger.Default.Send(new ObjectNotificationMessageAction<PnFChart>(
                          NotificationMessages.GetPnFChart,
                          objectId,
                          (result) =>
                          {
                              chart = result;
                          })
                          );
                    if (chart != null)
                    {
                        ObservableObject pointAndFigureChartViewModel = (ObservableObject)ViewModelLocator.Current.GetPointAndFigureChartViewModel(chart);
                        return pointAndFigureChartViewModel;
                    }
                }
            }
            return null;
        }


    }
}
