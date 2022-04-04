using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


namespace PnFData.Model
{
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

        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey(nameof(ShareId))]
        public Share Share { get; set; }

    }
}
