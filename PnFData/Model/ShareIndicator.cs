using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PnFData.Model
{
    [Flags]
    public enum ShareEvents
    {
        [Description("New Double Top")]
        NewDoubleTop = 0x0001,
        [Description("New Triple Top")]
        NewTripleTop = 0x0002,
        [Description("New RS Buy")]
        NewRsBuy = 0x0004,
        [Description("New Peer RS Buy")]
        NewPeerRsBuy = 0x0008,
        [Description("New Double Bottom")]
        NewDoubleBottom = 0x0010,
        [Description("New Triple Bottom")]
        NewTripleBottom = 0x0020,
        [Description("New RS Sell")]
        NewRsSell = 0x0040,
        [Description("New Peer RS Sell")]
        NewPeerRsSell = 0x0080,
        [Description("Closed Above EMA 10")]
        NewCloseAboveEma10 = 0x0100,
        [Description("Closed Above EMA 30")]
        NewCloseAboveEma30 = 0x0200,
        [Description("Dropped Below EMA 10")]
        NewDropBelowEma10 = 0x0400,
        [Description("Dropped Below EMA 30")]
        NewDropBelowEma30 = 0x0800,
        [Description("Breached Bull Support Line")]
        NewBullSupportBreach = 0x1000,
        [Description("52 Week High")]
        High52Week = 0x2000,
        [Description("52 Week Low")]
        Low52Week = 0x4000,
        [Description("All Signals")]
        AllShareSignals = NewDoubleTop|NewDoubleBottom|NewTripleTop|NewTripleBottom|NewBullSupportBreach|High52Week|Low52Week

    }


    [Index(nameof(Day))]
    public class ShareIndicator: EntityData
    {
        public DateTime Day { get; set; }

        public double? Ema10 {get; set;}

        public double? Ema30 {get;set;}

        public bool? ClosedAboveEma10 {get; set;}

        public bool? ClosedAboveEma30 {get; set;}

        public bool? Rising {get;set;}

        public bool? DoubleTop {get;set;}

        public bool? TripleTop {get;set;}

        public bool? RsRising {get;set;}

        public bool? RsBuy {get;set;}

        public bool? PeerRsRising {get;set;}

        public bool? PeerRsBuy {get;set;}

        public bool? Falling {get;set;}

        public bool? DoubleBottom {get;set;}

        public bool? TripleBottom {get;set;}

        public bool? RsFalling {get;set;}

        public bool? RsSell {get;set;}

        public bool? PeerRsFalling {get;set;}

        public bool? PeerRsSell {get;set;}

        public bool AboveBullSupport {get;set;}
        
        public int NewEvents{get;set;}

        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey(nameof(ShareId))]
        public Share Share { get; set; }

    }
}
