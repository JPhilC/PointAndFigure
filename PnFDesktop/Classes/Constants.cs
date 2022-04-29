using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.Classes
{
    internal static class Constants
    {
        public const float ChartMargin = 10f;
        public const float DefaultChartWidth = 400f;
        public const float DefaultChartHeight = 300f;
        public const string PointAndFigureChart = "PnFChart"; 
        public const string MarketSummary = "MarketSummary";
        public const string SharesSummary = "SharesSummary";
        public const string FilteredSharesSummary = "FilteredSharesSummary";

        #region Notifications ...
        public const string OpenIndexChartWindowLoaded = "OpenIndexChartWindowLoaded";
        public const string MarketSummaryUILoaded = "MarketSummaryUILoaded";
        public const string RefreshMarketSummary = "RefreshMarketSummary";
        public const string SharesSummaryUILoaded = "SharesSummaryUILoaded";
        public const string FilteredSharesSummaryUILoaded = "FilteredSharesSummaryUILoaded";
        public const string RefreshFilteredSharesSummary = "RefreshFilteredSharesSummary";
        public const string OpenSharesSummaryPage = "OpenSharesSummaryPage";

        #endregion
    }
}
