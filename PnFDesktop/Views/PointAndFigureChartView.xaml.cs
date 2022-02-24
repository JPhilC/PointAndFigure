using PnFDesktop.ViewCharts;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public RenderTargetBitmap RenderVisaulToBitmap(Visual vsual, int width, int height)
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

            RenderTargetBitmap rtb = this.RenderVisaulToBitmap(vsual, widhth, height);
            MemoryStream file = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(file);

            return file;
        }

        #endregion

    }
}
