using PnFDesktop.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Interaction logic for UserOptionsWindow.xaml
    /// </summary>
    public partial class UserOptionsWindow : Window
    {
        private UserOptionsViewModel _viewModel = null;
        private bool _vmClosing = false;

        public UserOptionsWindow()
        {
            InitializeComponent();
            _viewModel = (UserOptionsViewModel)this.DataContext;

            // Hook up to the viewmodels close action
            if (_viewModel.SaveAndCloseAction == null)
            {
                _viewModel.SaveAndCloseAction = new Action(() => {
                    this._vmClosing = true;
                    this.DialogResult = true;
                    this.Close();
                });
            }

            this.PreviewKeyDown += HandlePreviewKeyDown;
        }
        void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool cancel = false;
            if (!_vmClosing)
            {
                cancel = !_viewModel.SaveOptions();
            }
            if (cancel)
            {
                e.Cancel = cancel;
            }
            else
            {

                if (_viewModel.SaveAndCloseAction != null)
                {
                    _viewModel.SaveAndCloseAction = null;
                }
            }
        }

    }
}
