using System;
using System.Text;
using System.Windows;
using System.Windows.Media;
using PnFData.Model;

namespace PnFDesktop.Classes
{

    public enum BoxPropertyEnum
    {
        Tooltip,
        Drawing
    }

    public class BoxPropertyProvider
    {
        private static BoxPropertyProvider _Instance = null;
        private static BoxPropertyProvider Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new BoxPropertyProvider();
                }
                return _Instance;
            }

        }

        public static object GetProperty(PnFBox box, BoxPropertyEnum property)
        {
            return Instance.GetPropertyInternal(box, property);
        }

        private object GetPropertyInternal(PnFBox box, BoxPropertyEnum property)
        {
            string tooltip = "";
            string drawingKey="undefined";
            StringBuilder sb = new StringBuilder();
            switch (box.BoxType)
            {
                case PnFBoxType.O:
                    drawingKey = "OBox5x5";
                    sb.AppendLine("Going down");
                    break;
                case PnFBoxType.X:
                    drawingKey = "XBox5x5";
                    sb.AppendLine("Going up");
                    break;

                default:
                    sb.AppendLine("Unknown");
                    break;
            }

            tooltip = sb.ToString().TrimEnd('\r', '\n');

            // What we return depends on the property requested
            object propertyValue = null;
            switch (property)
            {
                case BoxPropertyEnum.Tooltip:
                    propertyValue = tooltip;
                    break;
                case BoxPropertyEnum.Drawing:
                    DrawingImage image = Application.Current.TryFindResource(drawingKey) as DrawingImage;
                    if (image == null)
                    {
                        // MessageLog.LogMessage(this, LogType.Error, string.Format("The artwork is missing for key '{0}'.", drawingKey));
                        image = Application.Current.TryFindResource("ImageKeyNotRecognised") as DrawingImage;
                    }
                    if (image != null)
                    {
                        propertyValue = image;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("property", "Unrecognised NodePropertyEnum value.");
            }

            return propertyValue;
        }
    }
}
