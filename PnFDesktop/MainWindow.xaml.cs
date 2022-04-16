using System;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.ViewCharts;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFDesktop.Classes;
using PnFDesktop.Config;
using PnFDesktop.Interfaces;
using PnFDesktop.Messaging;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace PnFDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int cursorRequestCount = 0;
        private MainViewModel _viewModel;
        private bool restored = false;
        const string SettingsKey = "MainWindow";
        bool exitCommandCalled = false;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = (MainViewModel)this.DataContext;

            // This stuff might be better handled in a more MVVM pattern
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

            Closing += (s, e) =>
            {
                if (!exitCommandCalled)
                {
                    if (ShutdownService.RequestShutdown())
                    {
                        FormLayoutManager.Current.SaveSettings(this, MainWindow.SettingsKey);
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            };
            // Hook up to the viewmodels close action
            if (_viewModel.ExitApplicationAction == null)
            {
                _viewModel.ExitApplicationAction = new Action(() =>
                {
                    if (ShutdownService.RequestShutdown())
                    {
                        FormLayoutManager.Current.SaveSettings(this, MainWindow.SettingsKey);
                        exitCommandCalled = true;
                        this.Close();
                    }
                });
            }

            WeakReferenceMessenger.Default.Register<NotificationMessageAction<string>>(this, (r,message) =>
            {

                // Is this a save workspace layout notification?
                if (message.Notification == LayoutNotificationMessages.GetWorkspaceLayout)
                {
                    string xmlLayoutString = string.Empty;

                    using (StringWriter fs = new StringWriter())
                    {
                        XmlLayoutSerializer xmlLayout = new XmlLayoutSerializer(this.dockingManager);

                        xmlLayout.Serialize(fs);

                        xmlLayoutString = fs.ToString();
                    }

                    message.Execute(xmlLayoutString);
                }
            });

            WeakReferenceMessenger.Default.Register<NotificationMessage<string>>(this, (r, message) =>
            {
                // Is this a load workspace layout notification?
                if (message.Notification == LayoutNotificationMessages.LoadWorkspaceLayout)
                {
                    StringReader sr = new StringReader(message.Content);

                    var layoutSerializer = new XmlLayoutSerializer(this.dockingManager);
                    layoutSerializer.LayoutSerializationCallback += UpdateLayout;
                    layoutSerializer.Deserialize(sr);
                }
            });

        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.Focus();
            //App.splashScreen.LoadComplete();
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.ScrollToEnd();
        }

        private void dockingManager_DocumentClosed(object sender, Xceed.Wpf.AvalonDock.DocumentClosedEventArgs e)
        {
            // Signal to anyone who is listening that a document has been closed.
            if (e.Document.Content != null)
            {
                if (e.Document.Content is PointAndFigureChartViewModel vm)
                {
                    WeakReferenceMessenger.Default.Send(new DocumentClosedMessage(this, vm.Chart.Id, e.Document.ContentId));
                }
            }
        }
        /// <summary>
        /// Convert a Avalondock ContentId into a viewmodel instance
        /// that represents a document or tool window. The re-load of
        /// this component is cancelled if the Id cannot be resolved.
        /// 
        /// The result is (viewmodel Id or Cancel) is returned in <paramref name="args"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UpdateLayout(object sender, LayoutSerializationCallbackEventArgs args)
        {
            var resolver = this.DataContext as IViewModelResolver;

            if (resolver == null)
                return;

            // Get a matching viewmodel for that view via DataContext property of this view
            ObservableObject content_view_model = resolver.ContentViewModelFromID(args.Model.ContentId);

            if (content_view_model == null)
                args.Cancel = true;

            // found a match - return it
            args.Content = content_view_model;
        }


        private void Window_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (!restored)
            {
                // Restore the layout and position of this window
                FormLayoutManager.Current.RestoreSettings(this, MainWindow.SettingsKey);
                restored = true;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Main window unloaded");
        }

    }
}
