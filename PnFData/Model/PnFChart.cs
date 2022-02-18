using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{

    public enum PnFChartSource
    {
        Share,
        Index,
        RSStockVMarket,
        RSStockVSector,
        RSSectorVMarket
    }

    public class PnFChart: EntityData
    {
        public PnFChartSource Source { get; set; }

        public DateTime GeneratedDate { get; set; }

        public double? BoxSize { get; set; }
        public int Reversal { get; set; }

        public List<PnFColumn> Columns { get; } = new List<PnFColumn>();

        public ShareChart ShareChart { get; set; }

        //public IndexChart IndexChart { get; set; }

    }
}
