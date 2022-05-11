using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PnFData.Model
{
    [Flags]
    public enum IndexEvents
    {
        [Description("Bull Alert")]
        BullAlert = 0x0001,
        [Description("Bear Alert")]
        BearAlert = 0x0002,
        [Description("Bull Confirmed")]
        BullConfirmed = 0x0004,
        [Description("Bear Confirmed")]
        BearConfirmed = 0x0008,
        [Description("Bull Confirmed < 30")]
        BullConfirmedLt30 = 0x0010,
        [Description("Bear Confirmed > 70")]
        BearConfirmedGt70 = 0x0020,
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
        public bool? PercentRsRisingRising { get; set; }
        public bool? PercentPositiveTrendRising { get; set; }
        public bool? PercentAbove30EmaRising { get; set; }
        public bool? PercentAbove10EmaRising { get; set; }
        public bool? BullishPercentFalling { get; set; }
        public bool? BullishPercentDoubleBottom { get; set; }
        public bool? PercentRSBuyFalling { get; set; }
        public bool? PercentRsRisingFalling { get; set; }
        public bool? PercentPositiveTrendFalling { get; set; }
        public bool? PercentAbove30EmaFalling { get; set; }
        public bool? PercentAbove10EmaFalling { get; set; }

        public int NewEvents {get;set;}


        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey(nameof(IndexId))]
        public Index Index { get; set; }

    }
}
