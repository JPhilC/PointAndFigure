using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Messaging;
using PnFDesktop.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Interaction logic for OpenShareChartWindow.xaml
    /// </summary>
    public partial class OpenIndexChartWindow : Window
    {
        private OpenIndexChartViewModel _viewModel;

        public OpenIndexChartWindow(OpenIndexChartViewModel viewModel)
        {
            _viewModel = viewModel;
            this.DataContext = _viewModel;

            InitializeComponent();
            // Hook up to the viewmodels close action
            if (_viewModel.SaveAndCloseAction == null)
            {
                _viewModel.SaveAndCloseAction = new Action(() =>
                {
                    this.DialogResult = true;
                    Close();
                });
            }
            if (_viewModel.CancelAndCloseAction == null)
            {
                _viewModel.CancelAndCloseAction = new Action(() =>
                {
                    this.DialogResult = false;
                    Close();
                });
            }
        }


        #region Save and restore position ...
        private bool _restored;

        protected override void OnClosing(CancelEventArgs e)
        {
            FormLayoutManager.Current.SaveSettings(this);
            base.OnClosing(e);
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            if (!_restored)
            {
                FormLayoutManager.Current.RestoreSettings(this);
                _restored = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.SaveAndCloseAction = null;
            _viewModel.CancelAndCloseAction = null;
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.OpenIndexChartWindowLoaded));
        }
    }
}
