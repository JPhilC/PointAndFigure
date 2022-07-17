using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PnFData.Model
{
    [Flags]
    public enum IndexEvents
    {
        [Description("Bullish % Bull Alert")]
        BullAlert = 0x0001,
        [Description("Bullish % Bear Alert")]
        BearAlert = 0x0002,
        [Description("Bullish % Bull Confirmed")]
        BullConfirmed = 0x0004,
        [Description("Bullish % Bear Confirmed")]
        BearConfirmed = 0x0008,
        [Description("Bullish % Bull Confirmed < 30")]
        BullConfirmedLt30 = 0x0010,
        [Description("Bullish % Bear Confirmed > 70")]
        BearConfirmedGt70 = 0x0020,
        [Description("Percent of 10 risen above 30")]
        PercentOf10Gt30 = 0x0040,
        [Description("Percent of 10 dropped below 70")]
        PercentOf10Lt70 = 0x0080,
        [Description("Percent of 30 risen above 30")]
        PercentOf30Gt30 = 0x0100,
        [Description("Percent of 30 dropped below 70")]
        PercentOf30Lt70 = 0x0200,
        [Description("High-low risen above 30")]
        HighLowGt30 = 0x0400,
        [Description("High-low dropped below 70")]
        HighLowLt70 = 0x0800,
        [Description("Percent RS X Buy")]
        PercentRsXBuy = 0x001000,
        [Description("Percent RS X Sell")]
        PercentRsXSell = 0x002000,
        [Description("Percent on RS Buy Buy")]
        PercentRsBuyBuy = 0x004000,
        [Description("Percent on RS Buy Sell")]
        PercentRsBuySell = 0x008000,
        [Description("PT Buy")]
        PercentPtBuy = 0x010000,
        [Description("PT Sell")]
        PercentPtSell = 0x020000,
        [Description("Percent Above EMA 10 Buy")]
        PercentEma10Buy = 0x040000,
        [Description("Percent Above EMA 10 Sell")]
        PercentEma10Sell = 0x080000,
        [Description("Percent Above EMA 30 Buy")]
        PercentEma30Buy = 0x100000,
        [Description("Percent Above EMA 30 Sell")]
        PercentEma30Sell = 0x200000,
        [Description("High Low Index Buy")]
        HiLoBuy = 0x400000,
        [Description("High Low Index Sell")]
        HiLoSell = 0x800000,
        [Description("RS Buy")]
        RsBuy = 0x1000000,
        [Description("RS Sell")]
        RsSell = 0x2000000
    }

    [Index(nameof(Day))]
    public class IndexIndicator : EntityData
    {
        public DateTime Day { get; set; }

        public bool? Rising { get; set; }

        public bool? Buy { get; set; }

        public bool? RsRising { get; set; }

        public bool? RsBuy { get; set; }

        public bool? Falling { get; set; }

        public bool? Sell { get; set; }

        public bool? RsFalling { get; set; }

        public bool? RsSell { get; set; }

        public bool? BullishPercentRising { get; set; }
        public bool? BullishPercentDoubleTop { get; set; }
        public bool? PercentRSBuyRising { get; set; }
        public bool? PercentRSBuyBuy { get; set; }
        public bool? PercentRsRisingRising { get; set; }
        public bool? PercentRsRisingBuy { get; set; }
        public bool? PercentPositiveTrendRising { get; set; }
        public bool? PercentPositiveTrendBuy { get; set; }
        public bool? PercentAbove30EmaRising { get; set; }
        public bool? PercentAbove30EmaBuy { get; set; }
        public bool? PercentAbove10EmaRising { get; set; }
        public bool? PercentAbove10EmaBuy { get; set; }
        public bool? BullishPercentFalling { get; set; }
        public bool? BullishPercentDoubleBottom { get; set; }
        public bool? PercentRSBuyFalling { get; set; }
        public bool? PercentRSBuySell { get; set; }
        public bool? PercentRsRisingFalling { get; set; }
        public bool? PercentRsRisingSell { get; set; }
        public bool? PercentPositiveTrendFalling { get; set; }
        public bool? PercentPositiveTrendSell { get; set; }
        public bool? PercentAbove30EmaFalling { get; set; }
        public bool? PercentAbove30EmaSell { get; set; }
        public bool? PercentAbove10EmaFalling { get; set; }
        public bool? PercentAbove10EmaSell { get; set; }
        public bool? HighLowIndexRising { get; set; }
        public bool? HighLowIndexBuy { get; set; }
        public bool? HighLowIndexFalling { get; set; }
        public bool? HighLowIndexSell { get; set; }

        public int NewEvents { get; set; }


        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey(nameof(IndexId))]
        public Index Index { get; set; }

    }
}
