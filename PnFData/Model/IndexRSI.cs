using Microsoft.EntityFrameworkCore;
using PnFData.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    [Index(nameof(Day))]
    public class IndexRSI : EntityData, IDayValue
    {

        public DateTime Day { get; set; }

        public double Value { get; set; }

        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey(nameof(IndexId))]
        public Index Index { get; set; }
    }
}
