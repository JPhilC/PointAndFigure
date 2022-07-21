using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public class IndexChart:EntityData
    {
        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey("IndexId")]
        public Index Index { get; set; }

        [Required] 
        public Guid ChartId { get; set; }

        [ForeignKey("ChartId")]
        public PnFChart Chart { get; set; }

    }
}