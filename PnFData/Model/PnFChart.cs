using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{

    public enum PnFChartSource
    {
        [Description("Share")]
        Share,
        [Description("Index")]
        Index,
        [Description("Stock RS")]
        RSStockVMarket,
        [Description("Peer RS")]
        RSStockVSector,
        [Description("Sector RS")]
        RSSectorVMarket
    }

    public class PnFChart: EntityData
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public PnFChartSource Source { get; set; }

        public DateTime GeneratedDate { get; set; }

        public double? BoxSize { get; set; }
        public int Reversal { get; set; }

        public List<PnFColumn> Columns { get; } = new List<PnFColumn>();

        public ShareChart? ShareChart { get; set; }

        public IndexChart? IndexChart { get; set; }

    }
}
