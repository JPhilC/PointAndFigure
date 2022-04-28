using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Controls;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using PnFDesktop.ViewCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PnFDesktop.ViewModels
{
    public class MainViewModel : ObservableObject, ILayoutViewModelParent, IViewModelResolver
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
                AdLayout = new AvalonDockLayoutViewModel(this);
            }

            WeakReferenceMessenger.Default.Register<DocumentClosedMessage>(this, (r, message) =>
            {
                if (message.Sender != this)
                {
                    // Remove the chart from the list of open documents.
                    PaneViewModel vm = DocumentPanes.FirstOrDefault(m => m.ContentId == message.DocumentId);
                    if (vm != null)
                    {
                        DocumentPanes.Remove(vm);
                    }
                }
            });


            WeakReferenceMessenger.Default.Register<ActiveDocumentChangedMessage>(this, (r, message) =>
            {
                if (message.Sender != this)
                {
                    if (message.DocumentType == ActiveDocumentType.PandFChart)
                    {
                        if (this.DocumentPanes.FirstOrDefault(c => c is IPointAndFigureChartViewModel && ((IPointAndFigureChartViewModel)c).ChartId == message.DocumentId) is IPointAndFigureChartViewModel pnfChartVm)
                        {
                            ActiveDocument = (PaneViewModel)pnfChartVm;
                            ActiveChart = pnfChartVm.Chart;
                            ActiveObject = ActiveChart;
                        }
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<NotificationMessage<MarketSummaryDTO>>(this, (r, message) =>
            {
                if (message.Sender != this)
                {
                    if (message.Notification == Constants.OpenSharesSummaryPage)
                    {
                        OpenSharesSummary(message.Content);
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<OpenPointAndFigureChartMessage>(this, async (r, message) =>
            {
                if (message.Sender != this)
                {
                    PnFChart? chart = await _dataService.GetPointAndFigureChartAsync(message.InstrumentId, message.ChartSource);
                    if (chart != null)
                    {
                        MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for {chart.Name} ...");
                        OpenPointAndFigureChart(chart, true);
                    }
                    else
                    {
                        MessageLog.LogMessage(this, LogType.Information, "Chart does not exist.");
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

        public AvalonDockLayoutViewModel AdLayout { get; } = null;

        #region DocumentPanes property ...
        ObservableCollection<PaneViewModel> _documentPanes;
        public ObservableCollection<PaneViewModel> DocumentPanes
        {
            get
            {
                if (_documentPanes == null)
                {
                    _documentPanes = new ObservableCollection<PaneViewModel>();
                }

                return _documentPanes;
            }
        }


        #endregion

        private PaneViewModel _activeDocument = null;


        public PaneViewModel ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (SetProperty(ref _activeDocument, value))
                {
                    UpdateActiveDocument(_activeDocument);
                }
            }
        }


        private void UpdateActiveDocument(PaneViewModel paneVm)
        {
            IPointAndFigureChartViewModel designerVm = paneVm as IPointAndFigureChartViewModel;
            if (designerVm != null)
            {
                ActiveChart = designerVm.Chart;
                ActiveObject = ActiveChart;
            }
        }

        private PnFChart _activePnFChart = null;

        /// <summary>
        /// Sets and gets the ActiveModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PnFChart ActiveChart
        {
            get => _activePnFChart;
            set
            {
                if (SetProperty(ref _activePnFChart, value))
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
            set => SetProperty(ref _activeObject, value);
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
            IPointAndFigureChartViewModel pnfChartDesignerViewModel = ViewModelLocator.Current.GetPointAndFigureChartViewModel(pnfChart, forceRefresh);
            if (pnfChartDesignerViewModel is PaneViewModel paneViewModel)
            {
                if (!this.DocumentPanes.Contains(paneViewModel))
                {
                    this.DocumentPanes.Add(paneViewModel);
                }
                if (makeActive)
                {
                    ActiveDocument = paneViewModel;
                }
            }
        }

        public void OpenSharesSummary(MarketSummaryDTO marketSummaryDTO)
        {
            SharesSummaryViewModel sharesSummaryViewModel = ViewModelLocator.GetSharesSummaryViewModel(marketSummaryDTO);
            if (sharesSummaryViewModel is PaneViewModel paneViewModel)
            {
                if (!this.DocumentPanes.Contains(paneViewModel))
                {
                    this.DocumentPanes.Add(paneViewModel);
                }
                ActiveDocument = paneViewModel;
            }
        }

        public void OpenMarketSummary()
        {
            // Get the ModelDesignerViewModel from the ViewModel locator instance. This is the definitive
            // source for viewpnfCharts.
            MarketSummaryViewModel marketSummaryViewModel = SimpleIoc.Default.GetInstance<MarketSummaryViewModel>();
            if (marketSummaryViewModel is PaneViewModel paneViewModel)
            {
                if (!this.DocumentPanes.Contains(paneViewModel))
                {
                    this.DocumentPanes.Add(paneViewModel);
                }
                ActiveDocument = paneViewModel;
            }
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

        private RelayCommand _openShareChartCommand;

        /// <summary>
        /// Close the current pnfChart display window
        /// </summary>
        public RelayCommand OpenShareChartCommand
        {
            get
            {
                return _openShareChartCommand
                       ?? (_openShareChartCommand = new RelayCommand(
                           async () =>
                           {
                               OpenShareChartViewModel openChartVm = SimpleIoc.Default.GetInstance<OpenShareChartViewModel>();
                               Window dialog = new OpenShareChartWindow(openChartVm);
                               dialog.Owner = Application.Current.MainWindow;
                               bool? dialogResult = dialog.ShowDialog();

                               if (dialogResult.HasValue && dialogResult.Value == true && openChartVm.SelectedShare != null)
                               {
                                   MessageLog.LogMessage(this, LogType.Information, $"Retrieving P & F chart data for {openChartVm.SelectedShare.Name} ...");
                                   //PnFChart? testChart = await _dataService.GetPointAndFigureChartAsync(new Guid("B9B46E45-2258-496D-9F6D-8D681A19926B"), PnFChartSource.RSSectorVMarket);
                                   PnFChart? testChart = await _dataService.GetPointAndFigureChartAsync(openChartVm.SelectedShare.Id, (PnFChartSource)openChartVm.ShareChartType);
                                   if (testChart != null)
                                   {
                                       MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for {openChartVm.SelectedShare.Name} ...");
                                       OpenPointAndFigureChart(testChart, true);
                                   }
                                   else
                                   {
                                       MessageLog.LogMessage(this, LogType.Information, "Chart does not exist.");
                                   }
                               }
                           }));
            }
        }

        private RelayCommand _openIndexChartCommand;

        /// <summary>
        /// Close the current pnfChart display window
        /// </summary>
        public RelayCommand OpenIndexChartCommand
        {
            get
            {
                return _openIndexChartCommand
                       ?? (_openIndexChartCommand = new RelayCommand(
                           async () =>
                           {
                               OpenIndexChartViewModel openChartVm = SimpleIoc.Default.GetInstance<OpenIndexChartViewModel>();
                               Window dialog = new OpenIndexChartWindow(openChartVm);
                               dialog.Owner = Application.Current.MainWindow;
                               bool? dialogResult = dialog.ShowDialog();

                               if (dialogResult.HasValue && dialogResult.Value == true && openChartVm.SelectedIndex != null)
                               {
                                   MessageLog.LogMessage(this, LogType.Information, $"Retrieving P & F chart data for {openChartVm.SelectedIndex.Description} ...");
                                   //PnFChart? testChart = await _dataService.GetPointAndFigureChartAsync(new Guid("B9B46E45-2258-496D-9F6D-8D681A19926B"), PnFChartSource.RSSectorVMarket);
                                   PnFChart? indexChart = await _dataService.GetPointAndFigureChartAsync(openChartVm.SelectedIndex.Id, (PnFChartSource)openChartVm.IndexChartType);
                                   if (indexChart != null)
                                   {
                                       MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for {openChartVm.SelectedIndex.Description} ...");
                                       OpenPointAndFigureChart(indexChart, true);
                                   }
                                   else
                                   {
                                       MessageLog.LogMessage(this, LogType.Information, "Chart does not exist.");
                                   }
                               }
                           }));
            }
        }


        private RelayCommand _openMarketSummaryCommand;

        /// <summary>
        /// Opens the market summary page
        /// </summary>
        public RelayCommand OpenMarketSummaryCommand
        {
            get
            {
                return _openMarketSummaryCommand
                       ?? (_openMarketSummaryCommand = new RelayCommand(
                           async () =>
                           {
                               MessageLog.LogMessage(this, LogType.Information, $"Opening market summary page ...");
                               OpenMarketSummary();
                           }));
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
                           () =>
                           {
                               // Pass on the message to whoever is listening and ready to do the printing
                               // most probably the CaseDesigner control since this has a handle on the visual.
                               WeakReferenceMessenger.Default.Send<PrintPointAndFigureChartMessage>(new PrintPointAndFigureChartMessage(this, ActiveChart.Id));
                           }, () =>
                           {
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

        ObservableObject? IViewModelResolver.ContentViewModelFromID(string contentId)
        {
            System.Diagnostics.Debug.WriteLine($"Resolving for content id:\"{contentId}\"");
            var anchorableVm = this.Tools.FirstOrDefault(d => d.ContentId == contentId);
            if (anchorableVm != null)
            {
                return anchorableVm;
            }

            var paneVm = this.DocumentPanes.FirstOrDefault(d => d.ContentId == contentId);
            if (paneVm != null)
            {
                return paneVm;
            }


            return this.OpenContentViewModel(contentId);
        }

        public ObservableObject? OpenContentViewModel(string contentId)
        {

            System.Diagnostics.Debug.WriteLine($"Opening for content id:\"{contentId}\"");
            if (string.IsNullOrEmpty(contentId)) return null;
            /*
            Resolving for content id:"ModelDesigner_a84db061-1a2c-494f-945b-7a5abf7cd8ca"
            Opening for content id:"ModelDesigner_a84db061-1a2c-494f-945b-7a5abf7cd8ca"
            Resolving for content id:"Chart_8a3a28ea-8d5e-40a4-849a-1727ac8a84e1"
            Opening for content id:"Chart_8a3a28ea-8d5e-40a4-849a-1727ac8a84e1"
            */
            string[] splitId = contentId.Split('_');
            if (splitId[0] == Constants.PointAndFigureChart)
            {
                if (splitId.Length == 2)
                {
                    Guid objectId = new Guid(splitId[1]);
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
                        PointAndFigureChartViewModel pointAndFigureChartViewModel = ViewModelLocator.Current.GetPointAndFigureChartViewModel(chart);
                        if (!this.DocumentPanes.Contains(pointAndFigureChartViewModel))
                        {
                            this.DocumentPanes.Add(pointAndFigureChartViewModel);
                        }
                        return pointAndFigureChartViewModel;
                    }
                }
            }
            else if (splitId[0] == Constants.MarketSummary)
            {
                MarketSummaryViewModel vm = SimpleIoc.Default.GetInstance<MarketSummaryViewModel>();
                if (!this.DocumentPanes.Contains(vm))
                {
                    this.DocumentPanes.Add(vm);
                }
                return vm;
            }
            return null;
        }


    }
}
