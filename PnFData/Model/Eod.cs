using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    [Index(nameof(Day))]
    public class Eod :EntityData
    {
        public DateTime Day { get; set; }

        public double Open { get; set; }

        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public double Volume { get; set; }

        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey(nameof(ShareId))]
        public Share Share { get; set; }
    }
}
