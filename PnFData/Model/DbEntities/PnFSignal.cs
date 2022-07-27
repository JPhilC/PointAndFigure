using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    [Flags]
    public enum PnFSignalEnum
    {
        NotSet              = 0x0000,
        IsRising            = 0x0001,       // Going up         [00000001]
        IsFalling           = 0x0002,       // Going down       [00000010]
        DoubleTop           = 0x0004,       // Double Bottom    [00000100]
        DoubleBottom        = 0x0008,       // Double Bottom    [00001000]
        TripleTop           = 0x0010,       // Triple Top       [00010000]
        TripleBottom        = 0x0020,       // Triple Bottom    [00100000]
        AboveBullishSupport = 0x0040        // Current box is abobe bullish support level [01000000]
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
