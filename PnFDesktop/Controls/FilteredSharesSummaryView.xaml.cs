﻿using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes;
using PnFDesktop.Messaging;
using PnFDesktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Interaction logic for FilteredSharesSummaryView.xaml
    /// </summary>
    public partial class FilteredSharesSummaryView : UserControl
    {
        public FilteredSharesSummaryView(FilteredSharesSummaryViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send<NotificationMessage>(new NotificationMessage(Constants.FilteredSharesSummaryUILoaded));
        }
    }
}
