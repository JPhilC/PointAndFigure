using System.ComponentModel.DataAnnotations;

namespace PnFData.Model
{
    public class Index:EntityData
    {
        [MaxLength(10)]
        public string ExchangeCode { get; set; }

        [MaxLength(4)]
        public string? ExchangeSubCode { get; set; }

        [MaxLength(100)]
        public string? SuperSector { get; set; }

        public List<IndexValue> IndexValues { get; } = new List<IndexValue>();
        
        public List<IndexRSI> RSIValues { get; } = new List<IndexRSI>();

        public List<IndexChart> Charts { get; } = new List<IndexChart>();

        public List<IndexIndicator> Indicators {get; } = new List<IndexIndicator>();
    }
}
