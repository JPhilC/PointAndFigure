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
        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart Chart { get; set; }

    }
}
