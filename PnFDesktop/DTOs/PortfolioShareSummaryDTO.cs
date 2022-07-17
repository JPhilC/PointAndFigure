using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class PortfolioShareSummaryDTO
    {
        public Guid Id { get; set; }
        public string? Tidm { get; set; }

        public string? Name { get; set; }
        
        public string? ExchangeCode { get; set; }

        public string? ExchangeSubCode { get; set; }

        public string? SuperSector { get; set; }

        public double? MarketCapMillions { get; set; }

        public double? Close {get; set;}

        public double? RsValue {get;set;}

        public double? PeerRsValue {get;set;}

        public double? Ema10 { get; set; }

        public double? Ema30 { get; set; }

        public bool? ClosedAboveEma10 { get; set; }

        public bool? ClosedAboveEma30 { get; set; }

        public bool? Rising { get; set; }

        public bool? DoubleTop { get; set; }

        public bool? TripleTop { get; set; }

        public bool? RsRising { get; set; }

        public bool? RsBuy { get; set; }

        public bool? PeerRsRising { get; set; }

        public bool? PeerRsBuy { get; set; }

        public bool? Falling { get; set; }

        public bool? DoubleBottom { get; set; }

        public bool? TripleBottom { get; set; }


        public bool? RsFalling { get; set; }

        public bool? RsSell { get; set; }

        public bool? PeerRsFalling { get; set; }

        public bool? PeerRsSell { get; set; }

        public bool? AboveBullSupport { get; set; }

        public double? WeeklyMomentum { get; set; }

        public bool? MomentumRising { get; set; }

        public bool? MomentumFalling { get; set; }

        public int? NewEvents {get;set;}

        public int? Score {get;set;}

        public string? Notices {get;set;}

        #region Market indicators ...

        public Guid MarketIndexId { get; set;}
        public double? MarketPercentRsBuy { get;set;}

        public double? MarketPercentRsRising { get;set;}

        public double? MarketPercentPositiveTrend { get;set;}

        public double? MarketBullishPercent {get;set;}

        public double? MarketPercentAboveEma10 { get; set; }

        public double? MarketPercentAboveEma30 { get; set; }
        public double? MarketHighLowIndexValue { get; set; }

        public bool? MarketRsRising { get; set; }

        public bool? MarketRsBuy { get; set; }

        public bool? MarketRsFalling { get; set; }

        public bool? MarketRsSell { get; set; }

        public bool? MarketBullishPercentRising { get; set; }
        public bool? MarketPercentRsBuyRising { get; set; }
        public bool? MarketPercentRsRisingRising { get; set; }
        public bool? MarketPercentPositiveTrendRising { get; set; }
        public bool? MarketPercentAbove30EmaRising { get; set; }
        public bool? MarketPercentAbove10EmaRising { get; set; }
        public bool? MarketBullishPercentFalling { get; set; }
        public bool? MarketPercentRsBuyFalling { get; set; }
        public bool? MarketPercentRsRisingFalling { get; set; }
        public bool? MarketPercentPositiveTrendFalling { get; set; }
        public bool? MarketPercentAbove30EmaFalling { get; set; }
        public bool? MarketPercentAbove10EmaFalling { get; set; }
        public bool? MarketHighLowIndexRising { get; set; }
        public bool? MarketHighLowIndexFalling { get; set; }

        public bool? MarketBullishPercentBuy { get; set; }
        public bool? MarketPercentRsBuyBuy { get; set; }
        public bool? MarketPercentRsRisingBuy { get; set; }
        public bool? MarketPercentPositiveTrendBuy { get; set; }
        public bool? MarketPercentAbove30EmaBuy { get; set; }
        public bool? MarketPercentAbove10EmaBuy { get; set; }
        public bool? MarketBullishPercentSell { get; set; }
        public bool? MarketPercentRsBuySell { get; set; }
        public bool? MarketPercentRsRisingSell { get; set; }
        public bool? MarketPercentPositiveTrendSell { get; set; }
        public bool? MarketPercentAbove30EmaSell { get; set; }
        public bool? MarketPercentAbove10EmaSell { get; set; }
        public bool? MarketHighLowIndexBuy { get; set; }
        public bool? MarketHighLowIndexSell { get; set; }

        public int? MarketNewEvents { get; set; }

        #endregion

        #region Sector indicators ...
        public Guid SectorIndexId { get; set;}
        public double? SectorBullishPercent { get; set; }
        public bool? SectorBullishPercentRising { get; set; }
        public bool? SectorBullishPercentFalling { get; set; }

        public bool? SectorBullishPercentBuy { get; set; }
        public bool? SectorBullishPercentSell { get; set; }

        public double? SectorRsValue { get; set; }

        public bool? SectorRsRising { get;set;}

        public bool? SectorRsBuy { get; set; }
        public bool? SectorRsFalling { get; set; }
        public bool? SectorRsSell { get; set; }

        public double? SectorPercentRsBuy { get; set; }

        public bool? SectorPercentRsBuyRising { get; set; }
        public bool? SectorPercentRsBuyFalling { get; set; }

        public bool? SectorPercentRsBuyBuy { get; set; }
        public bool? SectorPercentRsBuySell { get; set; }

        public double? SectorPercentRsRising { get; set; }
        public bool? SectorPercentRsRisingRising { get; set; }
        public bool? SectorPercentRsRisingFalling { get; set; }

        public bool? SectorPercentRsRisingBuy { get; set; }
        public bool? SectorPercentRsRisingSell { get; set; }


        public double? SectorPercentPositiveTrend { get; set; }
        public bool? SectorPercentPositiveTrendRising { get; set; }
        public bool? SectorPercentPositiveTrendFalling { get; set; }


        public bool? SectorPercentPositiveTrendBuy { get; set; }
        public bool? SectorPercentPositiveTrendSell { get; set; }


        public int? SectorNewEvents { get; set; }

        #endregion


    }
}
