using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PnFData.Model
{
    [Index("Day")]
    public class IndexValue : EntityData
    {

        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey("IndexId")]
        public Index Index { get; set; }
        
        public DateTime Day { get; set; }

        public double Value { get; set; }

        public int Contributors { get; set; }
    }
}
