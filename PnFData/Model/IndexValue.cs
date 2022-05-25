using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PnFData.Model
{
    [Index(nameof(Day))]
    public class IndexValue : EntityData
    {

        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey("IndexId")]
        public Index Index { get; set; }

        public DateTime Day { get; set; }

        public double Value { get; set; }

        public int Contributors { get; set; }

        public double? BullishPercent { get; set; }

        public double? PercentAboveEma10 { get; set; }

        public double? PercentAboveEma30 { get; set; }

        public double? PercentRsBuy { get; set; }

        public double? PercentRsRising { get; set; }

        public double? PercentPositiveTrend { get; set; }

        /// <summary>
        /// The percentage of new highs compared to new highs and lows
        /// </summary>
        public double? PercentHighLow {get;set;}

        /// <summary>
        /// The 10 day EMA of the percentHighLow figures
        /// </summary>
        public double? HighLowEma10 {get;set;}

    }
}
