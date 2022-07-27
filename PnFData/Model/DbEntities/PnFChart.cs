using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public enum PnFChartPriceScale
    {
        Normal,
        Logarithmic
    }

    public enum PnFChartSource
    {
        [Description("Share")]
        Share,
        [Description("Index")]
        Index,
        [Description("Share RS")]                                   // ShareRSI.
        RSStockVMarket,
        [Description("Peer RS")]
        RSStockVSector,
        [Description("Sector RS")]                                  // 4 - Sector RS
        RSSectorVMarket,
        [Description("Bullish Percent")]                            // 5 - IndexIndicator.BullishPercent
        IndexBullishPercent,
        [Description("Percent of Share RS on Buy")]                 // 6 - IndexIndicator.PercentShareRsBuy
        IndexPercentShareRsBuy,
        [Description("Percent of Share RS on X")]                   // 7 - IndexIndicator.PercentShareRsRising
        IndexPercentShareRsX,
        [Description("Percent of Shares with Positive Trends")]     // 8 - IndexIndicator.PercentSharePt
        IndexPercentSharePT,
        [Description("Percent of Shares above 30 EMA")]             // 9 - IndexIndicator.PercentAboveEma30
        IndexPercentShareAbove30,
        [Description("Percent of Shares bove 10 EMA")]              // 10 - IndexIndicator.PercentAboveEma10
        IndexPercentShareAbove10,
        [Description("High-Low Index")]                             // 11 - IndexIndicator. 10 day EMA of HighLow percentage
        HighLowIndex,
        [Description("Advance-Decline Line")]
        AdvanceDeclineLine
    }

    public class PnFChart : EntityData
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public PnFChartSource Source { get; set; }

        public DateTime GeneratedDate { get; set; }

        [DefaultValue(0)]
        public PnFChartPriceScale PriceScale { get; set; }

        public double? BoxSize { get; set; }

        /// <summary>
        /// Base value used for Logarithmic scale
        /// </summary>
        public double? BaseValue { get; set; }

        public PnFSignalEnum LastSignal { get; set; }

        public int Reversal { get; set; }

        public List<PnFColumn> Columns { get; } = new List<PnFColumn>();

        public List<PnFSignal> Signals { get; } = new List<PnFSignal>();

        public ShareChart? ShareChart { get; set; }

        public IndexChart? IndexChart { get; set; }

    }
}
