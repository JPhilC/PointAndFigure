using System;
using System.Windows.Data;
using System.Globalization;

namespace PnFDesktop.Classes
{
    public enum SignalOpacitySignal
    {
        Buy,
        Sell
    }

    /// <summary>
    /// Converts a double between 30 and 70 to an opacity value between 1.0 and 0.5 (buy) or 0.5 and 1.0 (sell)
    /// </summary>
    public class SignalToOpacityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a fraction to a percentage.
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return Binding.DoNothing ;}
            double lowerLimit = 30d;
            double upperLimit = 70d;
            double incomingValue = (double)value;
            SignalOpacitySignal signal = (SignalOpacitySignal)parameter;
            if (incomingValue < lowerLimit)
            {
                incomingValue = lowerLimit;
            }
            if (incomingValue > upperLimit)
            {
                incomingValue = upperLimit;
            }
            if (signal == SignalOpacitySignal.Buy)
            {
                double valueOffset = (incomingValue - lowerLimit)/(upperLimit - lowerLimit);
                double opacityOffset = valueOffset * 0.5;   // 
                return  1.0d-opacityOffset;
            }
            else // Sell
            {
                double valueOffset = (upperLimit -incomingValue) / (upperLimit - lowerLimit);
                double opacityOffset = valueOffset * 0.5;
                return 1.0d - opacityOffset;
            }
        }

        /// <summary>
        /// Convert a percentage back to a fraction.
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
