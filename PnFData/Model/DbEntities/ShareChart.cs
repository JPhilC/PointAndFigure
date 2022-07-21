using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public class ShareChart:EntityData
    {
        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey("ShareId")]
        public Share Share { get; set; }

        [Required] 
        public Guid ChartId { get; set; }

        [ForeignKey("ChartId")]
        public PnFChart Chart { get; set; }

    }
}
