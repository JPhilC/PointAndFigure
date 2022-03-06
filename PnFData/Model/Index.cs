using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
