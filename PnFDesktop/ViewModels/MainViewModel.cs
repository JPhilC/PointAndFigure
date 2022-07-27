using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using PnFData.Model;
using PnFData.Services;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PnFDesktop.ViewModels
{
    public class MainViewModel : ObservableObject, ILayoutViewModelParent, IViewModelResolver
    {

        public ObservableCollection<string> ExchangeCodes { get; } = new ObservableCollection<string>();

        public ObservableCollection<DayDTO> AvailableDays { get; } = new ObservableCollection<DayDTO>();

        public ObservableCollection<Portfolio> Portfolios { get; } = new ObservableCollection<Portfolio>();

        public bool PortfoliosAvailable
        {
            get => Portfolios.Any();
        }

        public Action ExitApplicationAction { get; set; }

        private Dictionary<string, Guid> _tempChartIds = new Dictionary<string, Guid>();

        private readonly IDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the WorkspaceViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            Task.WaitAll(new Task[]
            {
            Task.Run(async ()=>await LoadExchanges()),
            Task.Run(async ()=>await LoadAvailableDays()),
            Task.Run(async ()=>await LoadPortfolios())
            });

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
                    bool forceRefresh = false;
                    StdDevResult? stdDev = await _dataService.GetStandardDeviationAsync(message.InstrumentId, 50);   // Get 10 week Mean and StdDev
                    PnFChart? chart = null;
                    if (message.ChartSource == PnFChartSource.Share)    // By default generate a fresh up to date chart.
                    {
                        chart = await GenerateShareChart(message.Tidm);
                        if (chart != null)
                        {
                            Guid chartId;
                            if (!_tempChartIds.TryGetValue(message.Tidm, out chartId))
                            {
                                chartId = Guid.NewGuid();
                                _tempChartIds.Add(message.Tidm, chartId);
                            }
                            chart.Name = message.Name;
                            chart.Id = chartId;
                        }
                    }
                    else
                    {
                        chart = await _dataService.GetPointAndFigureChartAsync(message.InstrumentId, message.ChartSource);
                    }
                    if (chart != null)
                    {
                        MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for {chart.Name} ...");
                        OpenPointAndFigureChart(chart, stdDev, true);
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

        private async Task LoadExchanges()
        {
            var exchangeCodes = await _dataService.GetExchangeCodesAsync();
            if (exchangeCodes.Any())
            {
                ExchangeCodes.Clear();
                ExchangeCodes.Add("<All>");
                foreach (var exchangeCode in exchangeCodes)
                {
                    ExchangeCodes.Add(exchangeCode);
                }
            }
        }

        private async Task LoadAvailableDays()
        {
            var dates = await _dataService!.GetMarketAvailableDates(DateTime.Now.AddDays(-60));
            if (dates.Any())
            {
                AvailableDays.Clear();
                foreach (var dayDto in dates.OrderByDescending(d => d.Day))
                {
                    AvailableDays.Add(dayDto);
                }
            }
        }

        private async Task LoadPortfolios()
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            if (portfolios.Any())
            {
                Portfolios.Clear();
                foreach (var portfolio in portfolios)
                {
                    Portfolios.Add(portfolio);
                }
            }
            OnPropertyChanged("PortfoliosAvailable");
        }

        public void OpenPointAndFigureChart(PnFChart pnfChart, StdDevResult? stdDevResult, bool makeActive = false, bool forceRefresh = false)
        {
            double? mean = null;
            double? stdDev = null;
            if (stdDevResult != null)
            {
                mean = stdDevResult.Mean;
                stdDev = stdDevResult.StdDev;
            }
            // Get the ModelDesignerViewModel from the ViewModel locator instance. This is the definitive
            // source for viewpnfCharts.
            IPointAndFigureChartViewModel pnfChartDesignerViewModel = ViewModelLocator.Current.GetPointAndFigureChartViewModel(pnfChart, mean, stdDev, forceRefresh);
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

        public void OpenFilteredSharesSummary()
        {
            // Get the ModelDesignerViewModel from the ViewModel locator instance. This is the definitive
            // source for viewpnfCharts.
            FilteredSharesSummaryViewModel summaryViewModel = SimpleIoc.Default.GetInstance<FilteredSharesSummaryViewModel>();
            if (summaryViewModel is PaneViewModel paneViewModel)
            {
                if (!this.DocumentPanes.Contains(paneViewModel))
                {
                    this.DocumentPanes.Add(paneViewModel);
                }
                ActiveDocument = paneViewModel;
            }
        }

        public void CreatePortfolio()
        {
            CreatePortfolioViewModel createVm = SimpleIoc.Default.GetInstance<CreatePortfolioViewModel>();
            createVm.Portfolio = new Portfolio();
            Window dialog = new CreatePortfolioWindow(createVm);
            dialog.Owner = Application.Current.MainWindow;
            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value == true)
            {
                Portfolios.Add(createVm.Portfolio);         // Add to list for menu
                OnPropertyChanged("PortfoliosAvailable");   // Trigger menu update
                // Open the portfolio management pane for the newly created portfolio
                ManagePortfolio(createVm.Portfolio!.Id!);
            }
        }

        public void ManagePortfolio(Guid portfolioId)
        {
            Portfolio? portfolio = null;
            Task.WaitAll(new Task[] {
                        Task.Run(async () => {portfolio = await _dataService.GetPortfolioAsync(portfolioId);})
                        });
            if (portfolio != null)
            {
                PortfolioManagementViewModel portfolioManagementViewModel = ViewModelLocator.GetPortfolioManagementViewModel(portfolio);
                if (portfolioManagementViewModel is PaneViewModel paneViewModel)
                {
                    if (!this.DocumentPanes.Contains(paneViewModel))
                    {
                        this.DocumentPanes.Add(paneViewModel);
                    }
                    ActiveDocument = paneViewModel;
                }
            }
        }

        public void ViewPortfolio(Portfolio portfolio)
        {
            PortfolioSummaryViewModel portfolioVm = ViewModelLocator.GetPortfolioSummaryViewModel(portfolio);
            if (portfolioVm is PaneViewModel paneViewModel)
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


        private RelayCommand _importShareScopeCompaniesCommand;

        public RelayCommand ImportShareScopeCompaniesCommand
        {
            get
            {
                return _importShareScopeCompaniesCommand
                       ?? (_importShareScopeCompaniesCommand = new RelayCommand(
                           () =>
                           {
                               var dlg = new OpenFileDialog()
                               {
                                   Filter = "ShareScope Companies Export File|*.csv"
                               };
                               try
                               {
                                   if (dlg.ShowDialog().GetValueOrDefault())
                                   {
                                       MessageLog.LogMessage(null, LogType.Information, $"Importing company share data from {dlg.FileName}");
                                       var results = ShareScopeImportService.ImportShares(dlg.FileName);
                                       MessageLog.LogMessage(null, LogType.Information, $"Completed. {results.Item1} new records added, {results.Item2} records updated, {results.Item3} errors.");
                                   }
                               }
                               catch (Exception ex)
                               {
                                   MessageLog.LogMessage(null, LogType.Error, "There was an error during the import.", ex);
                               }
                           }));
            }
        }

        private RelayCommand _importShareScopeETFsCommand;

        public RelayCommand ImportShareScopeETFsCommand
        {
            get
            {
                return _importShareScopeETFsCommand
                       ?? (_importShareScopeETFsCommand = new RelayCommand(
                           () =>
                           {
                               var dlg = new OpenFileDialog()
                               {
                                   Filter = "ShareScope ETFs Export File|*.csv"
                               };
                               try
                               {
                                   if (dlg.ShowDialog().GetValueOrDefault())
                                   {
                                       MessageLog.LogMessage(null, LogType.Information, $"Importing ETF share data from {dlg.FileName}");
                                       var results = ShareScopeImportService.ImportETFs(dlg.FileName);
                                       MessageLog.LogMessage(null, LogType.Information, $"Completed. {results.Item1} new records added, {results.Item2} records updated, {results.Item3} errors.");
                                   }
                               }
                               catch (Exception ex)
                               {
                                   MessageLog.LogMessage(null, LogType.Error, "There was an error during the import.", ex);
                               }
                           }));
            }
        }


        #region Relay Commands ...
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

                               if (dialogResult.HasValue && dialogResult.Value == true)
                               {
                                   if (openChartVm.ShareChartType == ShareChartType.Share && openChartVm.UseRemoteData)
                                   {
                                       if (!string.IsNullOrEmpty(openChartVm.ShareTidm))
                                       {
                                           await GenerateAndOpenChart(openChartVm.ShareTidm, openChartVm.SelectedShare);
                                       }
                                       else
                                       {
                                           MessageLog.LogMessage(this, LogType.Warning, $"Share TIDM not specified.");
                                       }
                                   }
                                   else
                                   {
                                       if (openChartVm.SelectedShare != null)
                                       {
                                           await OpenStoredChart(openChartVm.SelectedShare, openChartVm.ShareChartType);
                                       }
                                       else
                                       {
                                           MessageLog.LogMessage(this, LogType.Warning, $"No share was selected.");
                                       }
                                   }
                               }
                           }));
            }
        }

        private async Task<PnFChart?> GenerateShareChart(string shareTidm)
        {
            PnFChart? chart = null;
            List<Eod> tickData = new List<Eod>();
            MessageLog.LogMessage(this, LogType.Information, $"Retrieving P & F chart data for TIDM '{shareTidm}' ...");
            try
            {
                var result = await AlphaVantageService.GetTimeSeriesDailyPrices(shareTidm, DateTime.Now.AddYears(-2), true);
                if (result.InError)
                {
                    MessageLog.LogMessage(this, LogType.Warning, $"There was an error downloading the price data. {result.Reason}");
                }
                else
                {
                    var dayPrices = result.Prices as Eod[] ?? result.Prices.ToArray();
                    if (dayPrices.Any())
                    {
                        foreach (Eod dayPrice in dayPrices)
                        {
                            tickData.Add(dayPrice);
                        }
                        DateTime maxDay = dayPrices.Max(p => p.Day);
                    }
                    else
                    {
                        MessageLog.LogMessage(this, LogType.Warning, $"No price data is available.");
                    }
                }
                if (tickData.Any())
                {
                    MessageLog.LogMessage(this, LogType.Information, $"Building P & F chart data for TIDM '{shareTidm}' ...");
                    PnFChartBuilderService chartBuilder = new PnFLogarithmicHiLoChartBuilderService(tickData);
                    chart = chartBuilder.BuildChart(2d, 3, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, $"There was an error downloading the data or generating the chart", ex);
            }
            return chart;
        }

        /// <summary>
        /// Download data from AlphaVantage API and build a chart.
        /// </summary>
        /// <param name="shareTidm"></param>
        /// <returns></returns>
        private async Task GenerateAndOpenChart(string shareTidm, ShareDTO? selectedShare)
        {
            PnFChart? chart = await GenerateShareChart(shareTidm);
            StdDevResult? stdDev = null;
            if (selectedShare != null)
            {
                stdDev = await _dataService.GetStandardDeviationAsync(selectedShare.Id, 50);
            }
            if (chart != null)
            {
                MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for TIDM '{shareTidm}' ...");
                OpenPointAndFigureChart(chart, stdDev, true);
            }
        }


        private async Task OpenStoredChart(ShareDTO selectedShare, ShareChartType shareChartType)
        {
            MessageLog.LogMessage(this, LogType.Information, $"Retrieving P & F chart data for {selectedShare.Name} ...");
            //PnFChart? testChart = await _dataService.GetPointAndFigureChartAsync(new Guid("B9B46E45-2258-496D-9F6D-8D681A19926B"), PnFChartSource.RSSectorVMarket);
            PnFChart? testChart = await _dataService.GetPointAndFigureChartAsync(selectedShare.Id, (PnFChartSource)shareChartType);
            StdDevResult? stdDev = await _dataService.GetStandardDeviationAsync(selectedShare.Id, 50);
            if (testChart != null)
            {
                MessageLog.LogMessage(this, LogType.Information, $"Generating P & F chart for {selectedShare.Name} ...");
                OpenPointAndFigureChart(testChart, stdDev, true);
            }
            else
            {
                MessageLog.LogMessage(this, LogType.Information, "Chart does not exist.");
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
                                       OpenPointAndFigureChart(indexChart, null, true);
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

        private RelayCommand _openFilteredSharesSummaryCommand;

        /// <summary>
        /// Opens the market summary page
        /// </summary>
        public RelayCommand OpenFilteredSharesSummaryCommand
        {
            get
            {
                return _openFilteredSharesSummaryCommand
                       ?? (_openFilteredSharesSummaryCommand = new RelayCommand(
                           async () =>
                           {
                               MessageLog.LogMessage(this, LogType.Information, $"Opening filtered shares summary page ...");
                               OpenFilteredSharesSummary();
                           }));
            }
        }

        private RelayCommand _createPortfolioCommand;

        /// <summary>
        /// Creates a new portfolio
        /// </summary>
        public RelayCommand CreatePortfolioCommand
        {
            get
            {
                return _createPortfolioCommand
                       ?? (_createPortfolioCommand = new RelayCommand(
                           () =>
                           {
                               CreatePortfolio();
                           }));
            }
        }

        private RelayCommand<Portfolio> _managePortfolioCommand;

        /// <summary>
        /// Creates a new portfolio
        /// </summary>
        public RelayCommand<Portfolio> ManagePortfolioCommand
        {
            get
            {
                return _managePortfolioCommand
                       ?? (_managePortfolioCommand = new RelayCommand<Portfolio>(
                           (portfolio) =>
                           {
                               if (portfolio != null)
                               {
                                   ManagePortfolio(portfolio.Id);
                               }
                           }));
            }
        }

        private RelayCommand<Portfolio> _viewPortfolioCommand;

        /// <summary>
        /// Creates a new portfolio
        /// </summary>
        public RelayCommand<Portfolio> ViewPortfolioCommand
        {
            get
            {
                return _viewPortfolioCommand
                       ?? (_viewPortfolioCommand = new RelayCommand<Portfolio>(
                           (portfolio) =>
                           {
                               if (portfolio != null)
                               {
                                   ViewPortfolio(portfolio);
                               }
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

        #endregion


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
                        PointAndFigureChartViewModel pointAndFigureChartViewModel = ViewModelLocator.Current.GetPointAndFigureChartViewModel(chart, null, null);
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
            else if (splitId[0] == Constants.FilteredSharesSummary)
            {
                FilteredSharesSummaryViewModel vm = SimpleIoc.Default.GetInstance<FilteredSharesSummaryViewModel>();
                if (!this.DocumentPanes.Contains(vm))
                {
                    this.DocumentPanes.Add(vm);
                }
                return vm;
            }
            else if (splitId[0] == Constants.PortfolioManagement)
            {
                if (splitId.Length == 2)
                {
                    Guid objectId = new Guid(splitId[1]);
                    Portfolio? portfolio = null;
                    Task.WaitAll(new Task[] {
                        Task.Run(async () => {portfolio = await _dataService.GetPortfolioAsync(objectId);})
                        });
                    if (portfolio != null)
                    {
                        PortfolioManagementViewModel portfolioManagementViewModel = ViewModelLocator.GetPortfolioManagementViewModel(portfolio);
                        if (!this.DocumentPanes.Contains(portfolioManagementViewModel))
                        {
                            this.DocumentPanes.Add(portfolioManagementViewModel);
                        }
                        return portfolioManagementViewModel;
                    }
                }
            }
            return null;
        }

    }
}
