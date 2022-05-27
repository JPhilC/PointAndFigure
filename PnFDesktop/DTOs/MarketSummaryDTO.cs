using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class MarketSummaryDTO
    {
        public Guid Id { get; set; }

        public DateTime Day { get; set; }

        public string? ExchangeCode { get; set; }

        public string? ExchangeSubCode { get; set; }

        public string? SuperSector { get; set; }

        public string? Description { get; set; }

        public double? Value { get; set; }

        public double? RsValue { get; set; }

        public int? Contributors { get; set; }

        public double? BullishPercent { get; set; }

        public double? PercentAboveEma10 { get; set; }

        public double? PercentAboveEma30 { get; set; }

        public double? PercentRsBuy { get; set; }

        public double? PercentRsRising { get; set; }

        public double? PercentPositiveTrend { get; set; }

        public double? HighLowIndexValue { get; set; }

        public bool? Rising { get; set; }

        public bool? Buy { get; set; }

        public bool? RsRising { get; set; }

        public bool? RsBuy { get; set; }

        public bool? Falling { get; set; }

        public bool? Sell { get; set; }

        public bool? RsFalling { get; set; }

        public bool? RsSell { get; set; }

        public bool? BullishPercentRising { get; set; }
        public bool? PercentRSBuyRising { get; set; }
        public bool? PercentRsRisingRising { get; set; }
        public bool? PercentPositiveTrendRising { get; set; }
        public bool? PercentAbove30EmaRising { get; set; }
        public bool? PercentAbove10EmaRising { get; set; }
        public bool? BullishPercentFalling { get; set; }
        public bool? PercentRSBuyFalling { get; set; }
        public bool? PercentRsRisingFalling { get; set; }
        public bool? PercentPositiveTrendFalling { get; set; }
        public bool? PercentAbove30EmaFalling { get; set; }
        public bool? PercentAbove10EmaFalling { get; set; }
        public bool? HighLowIndexRising { get; set; }
        public bool? HighLowIndexFalling { get; set; }

        public int? NewEvents { get; set; }

        public string? Notices {get; set;}

    }
}
