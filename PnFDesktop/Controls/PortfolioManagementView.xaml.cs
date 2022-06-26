using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Messaging;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Interaction logic for PortfolioSummaryView.xaml
    /// </summary>
    public partial class PortfolioManagementView : UserControl
    {
        public PortfolioManagementView(PortfolioManagementViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.PortfolioManagementUILoaded));
        }
    }
}
