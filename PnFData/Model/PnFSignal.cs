using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PnFData.Model
{
    [Flags]
    public enum PnFSignalEnum
    {
        NotSet              = 0x0000,
        IsRising            = 0x0001,       // Going up
        IsFalling           = 0x0002,       // Going down
        DoubleTop           = 0x0004,       // Double Bottom
        DoubleBottom        = 0x0008,       // Double Bottom
        TripleTop           = 0x0010,       // Triple Top
        TripleBottom        = 0x0020,       // Triple Bottom
        AboveBullishSupport = 0x0040        // Current box is abobe bullish support level
     }

    public class PnFSignal : EntityData
    {
        public DateTime Day { get; set; }

        public PnFSignalEnum Signals { get; set; }

        public double Value {set; get;} 
            
        [Required]
        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart PnFChart { get; set; }

    }
}
