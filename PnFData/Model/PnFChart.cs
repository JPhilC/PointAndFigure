using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{

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
        [Description("Sector RS")]                                  //
        RSSectorVMarket,
        [Description("Bullish Percent")]                            // IndexIndicator.BullishPercent
        IndexBullishPercent,
        [Description("Percent of Share RS on Buy")]                 // IndexIndicator.PercentShareRsBuy
        IndexPercentShareRsBuy,
        [Description("Percent of Share RS on X")]                   // IndexIndicator.PercentShareRsRising
        IndexPercentShareRsX,
        [Description("Percent of Shares with Positive Trends")]     // IndexIndicator.PercentSharePt
        IndexPercentSharePT,
        [Description("Percent of Shares above 30 EMA")]             // IndexIndicator.PercentAboveEma30
        IndexPercentShareAbove30,
        [Description("Percent of Shares bove 10 EMA")]              // IndexIndicator.PercentAboveEma10
        IndexPercentShareAbove10,
        [Description("High-Low Index")]
        HighLowIndex,
        [Description("Advance-Decline Line")]
        AdvanceDeclineLine
    }

    public class PnFChart: EntityData
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public PnFChartSource Source { get; set; }

        public DateTime GeneratedDate { get; set; }

        public double? BoxSize { get; set; }
        public int Reversal { get; set; }

        public List<PnFColumn> Columns { get; } = new List<PnFColumn>();

        public List<PnFSignal> Signals { get; } = new List<PnFSignal>();

        public ShareChart? ShareChart { get; set; }

        public IndexChart? IndexChart { get; set; }

    }
}
