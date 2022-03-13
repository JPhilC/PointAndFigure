using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PnFData.Model
{
    [Index(nameof(Tidm), IsUnique = true)]
    [Index(nameof(ShareDataSource), nameof(ShareDataSourceId), IsUnique = true)]
    public class Share : EntityData
    {
        [MaxLength(10)]
        public string Tidm { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string ExchangeCode { get; set; }

        public double SharesInIssueMillions { get; set; }

        public double MarketCapMillions { get; set; }

        [MaxLength(50)]
        public string SuperSector { get; set; }

        public string Sector { get; set; }

        [MaxLength(3)]
        public string PricesCurrency { get; set; }

        [MaxLength(5)]
        public string ShareDataSource { get; set; }

        public string ShareDataSourceId { get; set; }

        public DateTime? LastEodDate { get; set; }

        public bool EodError { get; set; }

        [MaxLength(4)]
        public string? ExchangeSubCode { get; set; }

        public List<Eod> EodPrices { get; } = new List<Eod>();

        public List<ShareRSI> RSIValues { get; } = new List<ShareRSI>();

        public List<ShareChart> Charts { get; } = new List<ShareChart>();

        public List<ShareIndicator> Indicators {get; } = new List<ShareIndicator>();
    }

    public class ShareSummary
    {
        public Guid Id { get; set; }
        public string Tidm { get; set; }
        public string Name { get; set; }

        public DateTime? LastEodDate { get; set; }
        public bool EodErrors { get; set; }

        public bool HasPrices { get; set; }
    }
}
