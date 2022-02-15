using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public class PnFColumn: EntityData
    {
        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart PnFChart { get; set; }

        public double Volume { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public List<PnFBox> Boxes { get; } = new List<PnFBox>();
    }
}
