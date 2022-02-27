using System;
using PnFDesktop.ViewCharts;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.Messaging;
using PnFDesktop.Classes.Messaging;
using PnFDesktop.Services;

namespace PnFDesktop.Views
{
    /// <summary>
    /// Interaction logic for PointAndFigureChartView.xaml
    /// </summary>
    public partial class PointAndFigureChartView : UserControl
    {
        private PointAndFigureChartViewModel _viewModel;
        private bool _initialLoad = true;


        public PointAndFigureChartView(PointAndFigureChartViewModel viewModel)
        {
            this.DataContext = viewModel;
            this._viewModel = viewModel;


            InitializeComponent();
            // Register to save model images and print models
            WeakReferenceMessenger.Default.Register<SavePointAndFigureChartAsImageMessage>(this, HandleSaveChartAsImageMessage);
            WeakReferenceMessenger.Default.Register<PrintPointAndFigureChartMessage>(this, HandlePrintChartMessage);

        }

        /// <summary>
        ///  Look for the Node sent in the message in the nodes list 
        /// </summary>
        private void HandleSaveChartAsImageMessage(object r, SavePointAndFigureChartAsImageMessage message)
        {
            if (message.Sender != this)
            {
                if (_viewModel.ChartId == message.ChartId)
                {
                    FrameworkElement displayControl = FindElement(this, "ZoomAndPanControl");
                    if (displayControl != null)
                    {
                        ImageDataService imageService = new ImageDataService();
                        imageService.SaveAsImage(displayControl, message.ImageFilename);
                    }
                }
            }
        }

        /// <summary>
        ///  Look for the Node sent in the message in the nodes list 
        /// </summary>
        private void HandlePrintChartMessage(object r, PrintPointAndFigureChartMessage message)
        {
            if (message.Sender != this)
            {
                if (_viewModel.ChartId == message.ChartId)
                {
                    FrameworkElement displayControl = FindElement(this, "ZoomAndPanControl");

                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                    dialog.PrintTicket = dialog.PrintQueue.DefaultPrintTicket;
                    dialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

                    if (dialog.ShowDialog() == true)
                    {
                        ImageDataService imageService = new ImageDataService();

                        Grid grid = new Grid();
                        grid.Margin = new Thickness(10);

                        //do this for each column
                        ColumnDefinition coldef;
                        coldef = new ColumnDefinition();
                        coldef.Width = new GridLength(dialog.PrintableAreaWidth, GridUnitType.Pixel);
                        grid.ColumnDefinitions.Add(coldef);

                        //do this for each row
                        RowDefinition rowdef;
                        rowdef = new RowDefinition();
                        rowdef.Height = new GridLength(1, GridUnitType.Auto);
                        grid.RowDefinitions.Add(rowdef);
                        //
                        rowdef = new RowDefinition();
                        rowdef.Height = new GridLength(1, GridUnitType.Auto);
                        grid.RowDefinitions.Add(rowdef);

                        TextBlock myTitle = new TextBlock();
                        myTitle.FontSize = 12;
                        myTitle.FontFamily = new FontFamily("Arial");
                        myTitle.TextAlignment = TextAlignment.Center;
                        myTitle.Text = _viewModel.Chart.Name;

                        grid.Children.Add(myTitle);
                        //put it in column 0, row 0
                        Grid.SetColumn(myTitle, 0);
                        Grid.SetRow(myTitle, 0);

                        Image pageImage = imageService.GetElementImage(displayControl);
                        pageImage.Stretch = Stretch.Uniform;
                        RenderOptions.SetBitmapScalingMode(pageImage, BitmapScalingMode.Fant);
                        grid.Children.Add(pageImage);
                        //put it in column 0, row 1
                        Grid.SetColumn(pageImage, 0);
                        Grid.SetRow(pageImage, 1);


                        grid.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));
                        grid.Arrange(new Rect(new Point(0, 0), grid.DesiredSize));

                        dialog.PrintVisual(grid, "Model design " + _viewModel.Chart.Name);
                    }
                }
            }
        }

        // Enumerate all the descendants of the visual object. 
        private FrameworkElement FindElement(FrameworkElement myVisual, string visualName)
        {
            if (myVisual == null)
            {
                return null;
            }
            FrameworkElement requiredVisual = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual); i++)
            {
                // Retrieve child visual at specified index value.
                DependencyObject childVisual = VisualTreeHelper.GetChild(myVisual, i);

                // Do processing of the child visual object. 
                FrameworkElement fe = childVisual as FrameworkElement;
                if (fe != null && fe.Name == visualName)
                {
                    requiredVisual = fe;
                }
                if (requiredVisual == null)
                {
                    // Enumerate children of the child visual object.
                    requiredVisual = FindElement(fe, visualName);
                }

                if (requiredVisual != null)
                {
                    break;
                }
            }
            return requiredVisual;
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    // Need this in model the element we want is nested
                    // in another element of the same type
                    foundChild = FindChild<T>(child, childName);
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            _viewModel.SetViewportSize(scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);
        }

        #region Helper methods ...
        public RenderTargetBitmap RenderVisualToBitmap(Visual vsual, int width, int height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap
                (width, height, 300, 300, PixelFormats.Default);
            rtb.Render(vsual);

            BitmapSource bsource = rtb;
            return rtb;
        }

        public MemoryStream GenerateImage(Visual vsual, int widhth, int height, string ext)
        {
            BitmapEncoder encoder = null;

            switch (ext.ToLower())
            {
                case "jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case "png":
                    encoder = new PngBitmapEncoder();
                    break;
                case "bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case "gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case "tif":
                    encoder = new TiffBitmapEncoder();
                    break;
            }

            if (encoder == null)
                return null;

            RenderTargetBitmap rtb = this.RenderVisualToBitmap(vsual, widhth, height);
            MemoryStream file = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(file);

            return file;
        }

        #endregion

        private void PointAndFigureChartView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                WeakReferenceMessenger.Default.Send<ActivePointAndFigureChartChangedMessage>(new ActivePointAndFigureChartChangedMessage(this, _viewModel.Chart));
            }
            else
            {
                WeakReferenceMessenger.Default.Send<ActivePointAndFigureChartChangedMessage>(new ActivePointAndFigureChartChangedMessage(this, null));
            }
        }
    }
}
