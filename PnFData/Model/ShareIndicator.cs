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

        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey(nameof(ShareId))]
        public Share Share { get; set; }

    }
}
