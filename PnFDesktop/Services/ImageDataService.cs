using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PnFDesktop.Classes;

namespace PnFDesktop.Services
{
    public class ImageDataService // : IImageDataService
    {
        private const int wpfDpi = 96;
        private const int outputDpi = 300;

        #region Saving image files...
        public static string ImageFileFilter = "Jpeg image file (.jpg)|*.jpg|Windows bitmap file (*.bmp)|*.bmp|Portable network graphics file (*.png)|*.png|Graphic interchange format (*.gif)|*.gif|Tagged image format (*.tif)|*.tif";
        public static string[] ImageExtensions = new string[] { ".jpg", ".bmp", ".png", ".gif", ".tif" };


        public Image GetElementImage(FrameworkElement visual)
        {
            BitmapEncoder encoder = new BmpBitmapEncoder();
            double width = visual.ActualWidth / wpfDpi * outputDpi;
            double height = visual.ActualHeight / wpfDpi * outputDpi;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, outputDpi, outputDpi, PixelFormats.Default);
            rtb.Render(visual);
            Image pageImage = new Image();
            pageImage.Source = rtb;
            pageImage.Stretch = Stretch.Uniform;
            return pageImage;
        }


        public bool SaveAsImage(FrameworkElement visual, string filename)
        {
            bool imageSaved = true;
            BitmapEncoder encoder = null;
            string ext = System.IO.Path.GetExtension(filename).ToLower();
            switch (ext) {
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".tif":
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                default:
                    MessageLog.LogMessage(this, LogType.Warning, "Unrecognised image file extension '" + ext + "'.");
                    break;
            }
            if (encoder == null) {
                return false;
            }
            try {
                double width = visual.ActualWidth / wpfDpi * outputDpi;
                double height = visual.ActualHeight / wpfDpi * outputDpi;
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, outputDpi, outputDpi, PixelFormats.Default);
                rtb.Render(visual);
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using (FileStream fStream = File.OpenWrite(filename)) {
                    encoder.Save(fStream);
                    MessageLog.LogMessage(this, LogType.Information, string.Format("Image file '{0}' has been saved.", Path.GetFileName(filename)));
                }
            }
            catch (Exception ex) {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred saving the image file, please see below:", ex);
                imageSaved = false;
            }
            return imageSaved;
        }

        private RenderTargetBitmap RenderVisaulToBitmap(Visual vsual, int width, int height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap
              (width, height, 96, 96, PixelFormats.Default);
            rtb.Render(vsual);

            BitmapSource bsource = rtb;
            return rtb;
        }


        #endregion

    }
}
