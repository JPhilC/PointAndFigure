using Microsoft.EntityFrameworkCore;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.Services
{
    public class DataService : IDataService
    {
        public PnFChart GetPointAndFigureChart(string tidm, int reversal)
        {
            return ChartBuilderService.GenerateHiLoChart(tidm, reversal, DateTime.Now.Date);
        }

        public PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal)
        {
            throw new NotImplementedException();
        }

        public async Task<PnFChart?> GetPointAndFigureChartAsync(Guid itemId, PnFChartSource chartSource)
        {
            PnFChart chart = null;
            try
            {
                using (var db = new PnFDataContext())
                {
                    switch (chartSource)
                    {
                        case PnFChartSource.Share:
                        case PnFChartSource.RSStockVMarket:
                        case PnFChartSource.RSStockVSector:
                            // Get the share
                            var share = await db.Shares
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefaultAsync(s => s.Id == itemId);


                            // Try retrieving the existing chart
                            if (share != null)
                            {
                                if (share.Charts.Any())
                                {
                                    var maxDay = share.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.Source == chartSource).Max(c => c.Chart.CreatedAt);

                                    var shareChart = share.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.Source == chartSource
                                        && c.Chart.CreatedAt == maxDay).FirstOrDefault();
                                    if (shareChart != null)
                                    {
                                        MessageLog.LogMessage(this, LogType.Information, $"Downloading share chart data for '{shareChart.ChartId}' ...");

                                        chart = await db.PnFCharts
                                            .Include(cc => cc.Columns.OrderBy(c => c.Index)).ThenInclude(cb => cb.Boxes.OrderBy(b => b.Index))
                                            .SingleOrDefaultAsync(c => c.Id == shareChart.ChartId);
                                    }
                                }

                            }
                            break;

                        default:
                            // Get the index
                            var index = await db.Indices
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefaultAsync(s => s.Id == itemId);

                            // Try retrieving the existing chart
                            if (index != null)
                            {
                                if (index.Charts.Any())
                                {
                                    var maxDay = index.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.Source == chartSource).Max(c => c.Chart.GeneratedDate);
                                    var indexChart = index.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.Source == chartSource
                                        && c.Chart.GeneratedDate == maxDay).FirstOrDefault();
                                    if (indexChart != null)
                                    {
                                        MessageLog.LogMessage(this, LogType.Information, $"Downloading index chart data for '{indexChart.ChartId}' ...");
                                        chart = await db.PnFCharts
                                            .Include(cc => cc.Columns.OrderBy(c => c.Index)).ThenInclude(cb => cb.Boxes.OrderBy(b => b.Index))
                                            .SingleOrDefaultAsync(c => c.Id == indexChart.ChartId);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(null, LogType.Error, "An error occurred loading the PnF chart data", ex);
            }
            return chart;
        }

        public async Task<IEnumerable<ShareDTO>> GetSharesAsync()
        {
            IEnumerable<ShareDTO> shares = new List<ShareDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    shares = await db.Shares.Select(s => new ShareDTO()
                    {
                        Id = s.Id,
                        Tidm = s.Tidm,
                        Name = s.Name
                    })
                    .OrderBy(s => s.Tidm)
                    .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share data", ex);
            }
            return shares;
        }

        /// <summary>
        /// Returns a single share record matching the tidm
        /// </summary>
        public async Task<Share?> GetShareAsync(string tidm)
        {
            Share share = null;
            try
            {
                using var db = new PnFDataContext();
                share = await db.Shares.FirstOrDefaultAsync(s => s.Tidm == tidm);
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share data", ex);
            }
            return share;
        }

        public async Task<IEnumerable<Portfolio>> GetPortfoliosAsync()
        {
            IEnumerable<Portfolio> portfolios = new List<Portfolio>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    portfolios = await db.Portfolios.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the portfolio data", ex);
            }
            return portfolios;
        }

        public async Task<Portfolio?> GetPortfolioAsync(Guid itemId)
        {
            Portfolio portfolio = null;
            try
            {
                using (var db = new PnFDataContext())
                {
                    portfolio = await db.Portfolios
                        .Include(ps => ps.Shares).ThenInclude(s => s.Share)
                        .SingleOrDefaultAsync(p => p.Id == itemId);
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the portfolio data", ex);
            }
            return portfolio;
        }



        public async Task<IEnumerable<PortfolioShareDTO>> GetPortfolioSharesAsync(Portfolio portfolio)
        {
            IEnumerable<PortfolioShareDTO> shares = new List<PortfolioShareDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    shares = await (from ps in db.PortfolioShares
                                    join s in db.Shares on ps.ShareId equals s.Id
                                    where ps.PortfolioId == portfolio.Id
                                    orderby s.Tidm
                                    select new PortfolioShareDTO()
                                    {
                                        Id = s.Id,
                                        Tidm = s.Tidm,
                                        Name = s.Name,
                                        Holding = ps.Holding
                                    }).ToListAsync();


                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share data", ex);
            }
            return shares;
        }

        public async Task<IEnumerable<IndexDTO>> GetIndicesAsync()
        {
            IEnumerable<IndexDTO> indices = new List<IndexDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    indices = await db.Indices.Select(i => new IndexDTO()
                    {
                        Id = i.Id,
                        ExchangeCode = i.ExchangeCode,
                        ExchangeSubCode = i.ExchangeSubCode,
                        SuperSector = i.SuperSector,
                        Description = (i.SuperSector == null ?
                            $"Market - {i.ExchangeCode}/{i.ExchangeSubCode}" :
                            $"Sector - {i.ExchangeCode}/{i.ExchangeSubCode}, {i.SuperSector}")
                    })
                    .OrderBy(i => i.SuperSector)
                    .ThenBy(i => i.ExchangeCode)
                    .ThenBy(i => i.ExchangeSubCode)
                    .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the index data", ex);
            }
            return indices;
        }

        public async Task<IEnumerable<DayDTO>> GetMarketAvailableDates(DateTime cutOff)
        {
            IEnumerable<DayDTO> dates = new List<DayDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    dates = await db.IndexValues
                        .Where(iv => iv.Day >= cutOff)
                        .Select(iv => new DayDTO() { Day = iv.Day })
                        .Distinct()
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the market available dates", ex);
            }
            return dates;
        }

        public async Task<IEnumerable<MarketSummaryDTO>> GetSectorValuesAsync(DateTime day, string exchangeCode)
        {
            IEnumerable<MarketSummaryDTO> indices = new List<MarketSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    if (exchangeCode == "<All>")
                    {
                        indices = await (from i in db.Indices
                                         from iv in db.IndexValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         from ii in db.IndexIndicators.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         from irs in db.IndexRSIValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         where i.SuperSector != null
                                         orderby i.SuperSector, i.ExchangeCode, i.ExchangeSubCode
                                         select new MarketSummaryDTO()
                                         {
                                             Id = i.Id,
                                             Day = day,
                                             ExchangeCode = i.ExchangeCode,
                                             ExchangeSubCode = i.ExchangeSubCode,
                                             SuperSector = i.SuperSector,
                                             Description = (i.SuperSector == null ?
                                                  $"Market - {i.ExchangeCode}/{i.ExchangeSubCode}" :
                                                  $"Sector - {i.ExchangeCode}/{i.ExchangeSubCode}, {i.SuperSector}"),
                                             Value = iv.Value,
                                             RsValue = irs.Value,
                                             Contributors = iv.Contributors,
                                             BullishPercent = iv.BullishPercent,
                                             PercentAboveEma10 = iv.PercentAboveEma10,
                                             PercentAboveEma30 = iv.PercentAboveEma30,
                                             PercentRsBuy = iv.PercentRsBuy,
                                             PercentRsRising = iv.PercentRsRising,
                                             PercentPositiveTrend = iv.PercentPositiveTrend,
                                             HighLowIndexValue = iv.HighLowEma10,
                                             Rising = ii.Rising,
                                             Buy = ii.Buy,
                                             RsRising = ii.RsRising,
                                             RsBuy = ii.RsBuy,
                                             Falling = ii.Falling,
                                             Sell = ii.Sell,
                                             RsFalling = ii.RsFalling,
                                             RsSell = ii.RsSell,
                                             BullishPercentRising = ii.BullishPercentRising,
                                             BullishPercentBuy = ii.BullishPercentDoubleTop,
                                             PercentRSBuyRising = ii.PercentRSBuyRising,
                                             PercentRSBuyBuy = ii.PercentRSBuyBuy,
                                             PercentRsRisingRising = ii.PercentRsRisingRising,
                                             PercentRsRisingBuy = ii.PercentRsRisingBuy,
                                             PercentPositiveTrendRising = ii.PercentPositiveTrendRising,
                                             PercentPositiveTrendBuy = ii.PercentPositiveTrendBuy,
                                             PercentAbove30EmaRising = ii.PercentAbove30EmaRising,
                                             PercentAbove30EmaBuy = ii.PercentAbove30EmaBuy,
                                             PercentAbove10EmaRising = ii.PercentAbove10EmaRising,
                                             PercentAbove10EmaBuy = ii.PercentAbove10EmaBuy,
                                             BullishPercentFalling = ii.BullishPercentFalling,
                                             BullishPercentSell = ii.BullishPercentDoubleBottom,
                                             PercentRSBuyFalling = ii.PercentRSBuyFalling,
                                             PercentRSBuySell = ii.PercentRSBuySell,
                                             PercentRsRisingFalling = ii.PercentRsRisingFalling,
                                             PercentRsRisingSell = ii.PercentRsRisingSell,
                                             PercentPositiveTrendFalling = ii.PercentPositiveTrendFalling,
                                             PercentPositiveTrendSell = ii.PercentPositiveTrendSell,
                                             PercentAbove30EmaFalling = ii.PercentAbove30EmaFalling,
                                             PercentAbove30EmaSell = ii.PercentAbove30EmaSell,
                                             PercentAbove10EmaFalling = ii.PercentAbove10EmaFalling,
                                             PercentAbove10EmaSell = ii.PercentAbove10EmaSell,
                                             HighLowIndexRising = ii.HighLowIndexRising,
                                             HighLowIndexBuy = ii.HighLowIndexBuy,
                                             HighLowIndexFalling = ii.HighLowIndexFalling,
                                             HighLowIndexSell = ii.HighLowIndexSell,
                                             NewEvents = ii.NewEvents,
                                             Score = 0 + (ii.BullishPercentRising == true ? 1 : 0)
                                                         + (ii.BullishPercentFalling == true ? -1 : 0)
                                                         + (ii.BullishPercentDoubleTop == true ? 1 : 0)
                                                         + (ii.BullishPercentDoubleBottom == true ? -1 : 0)
                                                         + (ii.RsRising == true ? 1 : 0)
                                                         + (ii.RsFalling == true ? -1 : 0)
                                                         + (ii.RsBuy == true ? 1 : 0)
                                                         + (ii.RsSell == true ? -1 : 0)
                                                         + (ii.PercentRSBuyRising == true ? 1 : 0)
                                                         + (ii.PercentRSBuyFalling == true ? -1 : 0)
                                                         + (ii.PercentRSBuyBuy == true ? 1 : 0)
                                                         + (ii.PercentRSBuySell == true ? -1 : 0)
                                                         + (ii.PercentRsRisingRising == true ? 1 : 0)
                                                         + (ii.PercentRsRisingFalling == true ? -1 : 0)
                                                         + (ii.PercentRsRisingBuy == true ? 1 : 0)
                                                         + (ii.PercentRsRisingSell == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendRising == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendFalling == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendBuy == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendSell == true ? -1 : 0),
                                             Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsBuy) == (int)IndexEvents.RsBuy ? "RS Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "RSX Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "RS Buy Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "PT Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsSell) == (int)IndexEvents.RsSell ? "RS Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "RSX Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "RS Buy Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "PT Sell " : "")
                                         }).ToListAsync();
                    }
                    else
                    {
                        indices = await (from i in db.Indices
                                         from iv in db.IndexValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         from ii in db.IndexIndicators.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         from irs in db.IndexRSIValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                         where i.ExchangeCode == exchangeCode && i.SuperSector != null
                                         orderby i.SuperSector, i.ExchangeCode, i.ExchangeSubCode
                                         select new MarketSummaryDTO()
                                         {
                                             Id = i.Id,
                                             Day = day,
                                             ExchangeCode = i.ExchangeCode,
                                             ExchangeSubCode = i.ExchangeSubCode,
                                             SuperSector = i.SuperSector,
                                             Description = (i.SuperSector == null ?
                                                  $"Market - {i.ExchangeCode}/{i.ExchangeSubCode}" :
                                                  $"Sector - {i.ExchangeCode}/{i.ExchangeSubCode}, {i.SuperSector}"),
                                             Value = iv.Value,
                                             RsValue = irs.Value,
                                             Contributors = iv.Contributors,
                                             BullishPercent = iv.BullishPercent,
                                             PercentAboveEma10 = iv.PercentAboveEma10,
                                             PercentAboveEma30 = iv.PercentAboveEma30,
                                             PercentRsBuy = iv.PercentRsBuy,
                                             PercentRsRising = iv.PercentRsRising,
                                             PercentPositiveTrend = iv.PercentPositiveTrend,
                                             HighLowIndexValue = iv.HighLowEma10,
                                             Rising = ii.Rising,
                                             Buy = ii.Buy,
                                             RsRising = ii.RsRising,
                                             RsBuy = ii.RsBuy,
                                             Falling = ii.Falling,
                                             Sell = ii.Sell,
                                             RsFalling = ii.RsFalling,
                                             RsSell = ii.RsSell,
                                             BullishPercentRising = ii.BullishPercentRising,
                                             BullishPercentBuy = ii.BullishPercentDoubleTop,
                                             PercentRSBuyRising = ii.PercentRSBuyRising,
                                             PercentRSBuyBuy = ii.PercentRSBuyBuy,
                                             PercentRsRisingRising = ii.PercentRsRisingRising,
                                             PercentRsRisingBuy = ii.PercentRsRisingBuy,
                                             PercentPositiveTrendRising = ii.PercentPositiveTrendRising,
                                             PercentPositiveTrendBuy = ii.PercentPositiveTrendBuy,
                                             PercentAbove30EmaRising = ii.PercentAbove30EmaRising,
                                             PercentAbove30EmaBuy = ii.PercentAbove30EmaBuy,
                                             PercentAbove10EmaRising = ii.PercentAbove10EmaRising,
                                             PercentAbove10EmaBuy = ii.PercentAbove10EmaBuy,
                                             BullishPercentFalling = ii.BullishPercentFalling,
                                             BullishPercentSell = ii.BullishPercentDoubleBottom,
                                             PercentRSBuyFalling = ii.PercentRSBuyFalling,
                                             PercentRSBuySell = ii.PercentRSBuySell,
                                             PercentRsRisingFalling = ii.PercentRsRisingFalling,
                                             PercentRsRisingSell = ii.PercentRsRisingSell,
                                             PercentPositiveTrendFalling = ii.PercentPositiveTrendFalling,
                                             PercentPositiveTrendSell = ii.PercentPositiveTrendSell,
                                             PercentAbove30EmaFalling = ii.PercentAbove30EmaFalling,
                                             PercentAbove30EmaSell = ii.PercentAbove30EmaSell,
                                             PercentAbove10EmaFalling = ii.PercentAbove10EmaFalling,
                                             PercentAbove10EmaSell = ii.PercentAbove10EmaSell,
                                             HighLowIndexRising = ii.HighLowIndexRising,
                                             HighLowIndexBuy = ii.HighLowIndexBuy,
                                             HighLowIndexFalling = ii.HighLowIndexFalling,
                                             HighLowIndexSell = ii.HighLowIndexSell,
                                             NewEvents = ii.NewEvents,
                                             Score = 0 + (ii.BullishPercentRising == true ? 1 : 0)
                                                         + (ii.BullishPercentFalling == true ? -1 : 0)
                                                         + (ii.BullishPercentDoubleTop == true ? 1 : 0)
                                                         + (ii.BullishPercentDoubleBottom == true ? -1 : 0)
                                                         + (ii.RsRising == true ? 1 : 0)
                                                         + (ii.RsFalling == true ? -1 : 0)
                                                         + (ii.RsBuy == true ? 1 : 0)
                                                         + (ii.RsSell == true ? -1 : 0)
                                                         + (ii.PercentRSBuyRising == true ? 1 : 0)
                                                         + (ii.PercentRSBuyFalling == true ? -1 : 0)
                                                         + (ii.PercentRSBuyBuy == true ? 1 : 0)
                                                         + (ii.PercentRSBuySell == true ? -1 : 0)
                                                         + (ii.PercentRsRisingRising == true ? 1 : 0)
                                                         + (ii.PercentRsRisingFalling == true ? -1 : 0)
                                                         + (ii.PercentRsRisingBuy == true ? 1 : 0)
                                                         + (ii.PercentRsRisingSell == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendRising == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendFalling == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendBuy == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendSell == true ? -1 : 0),
                                             Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsBuy) == (int)IndexEvents.RsBuy ? "RS Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "RSX Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "RS Buy Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "PT Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.HiLoBuy) == (int)IndexEvents.HiLoBuy ? "Hi-Lo Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsSell) == (int)IndexEvents.RsSell ? "RS Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "RSX Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "RS Buy Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "PT Sell " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.HiLoSell) == (int)IndexEvents.HiLoSell ? "Hi-Lo Sell " : "")
                                         }).ToListAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the index data", ex);
            }
            return indices;
        }

        public async Task<IEnumerable<MarketSummaryDTO>> GetMarketValuesAsync(DateTime day)
        {
            IEnumerable<MarketSummaryDTO> indices = new List<MarketSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    indices = await (from i in db.Indices
                                     from iv in db.IndexValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                     from ii in db.IndexIndicators.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                     from irs in db.IndexRSIValues.Where(r => r.IndexId == i.Id && r.Day == day).DefaultIfEmpty()
                                     where i.SuperSector == null
                                     orderby i.ExchangeCode, i.ExchangeSubCode
                                     select new MarketSummaryDTO()
                                     {
                                         Id = i.Id,
                                         Day = day,
                                         ExchangeCode = i.ExchangeCode,
                                         ExchangeSubCode = i.ExchangeSubCode,
                                         SuperSector = i.SuperSector,
                                         Description = (i.SuperSector == null ?
                                                  $"Market - {i.ExchangeCode}/{i.ExchangeSubCode}" :
                                                  $"Sector - {i.ExchangeCode}/{i.ExchangeSubCode}, {i.SuperSector}"),
                                         Value = iv.Value,
                                         RsValue = irs.Value,
                                         Contributors = iv.Contributors,
                                         BullishPercent = iv.BullishPercent,
                                         PercentAboveEma10 = iv.PercentAboveEma10,
                                         PercentAboveEma30 = iv.PercentAboveEma30,
                                         PercentRsBuy = iv.PercentRsBuy,
                                         PercentRsRising = iv.PercentRsRising,
                                         PercentPositiveTrend = iv.PercentPositiveTrend,
                                         HighLowIndexValue = iv.HighLowEma10,
                                         Rising = ii.Rising,
                                         Buy = ii.Buy,
                                         RsRising = ii.RsRising,
                                         RsBuy = ii.RsBuy,
                                         Falling = ii.Falling,
                                         Sell = ii.Sell,
                                         RsFalling = ii.RsFalling,
                                         RsSell = ii.RsSell,
                                         BullishPercentRising = ii.BullishPercentRising,
                                         BullishPercentBuy = ii.BullishPercentDoubleTop,
                                         PercentRSBuyRising = ii.PercentRSBuyRising,
                                         PercentRSBuyBuy = ii.PercentRSBuyBuy,
                                         PercentRsRisingRising = ii.PercentRsRisingRising,
                                         PercentRsRisingBuy = ii.PercentRsRisingBuy,
                                         PercentPositiveTrendRising = ii.PercentPositiveTrendRising,
                                         PercentPositiveTrendBuy = ii.PercentPositiveTrendBuy,
                                         PercentAbove30EmaRising = ii.PercentAbove30EmaRising,
                                         PercentAbove30EmaBuy = ii.PercentAbove30EmaBuy,
                                         PercentAbove10EmaRising = ii.PercentAbove10EmaRising,
                                         PercentAbove10EmaBuy = ii.PercentAbove10EmaBuy,
                                         BullishPercentFalling = ii.BullishPercentFalling,
                                         BullishPercentSell = ii.BullishPercentDoubleBottom,
                                         PercentRSBuyFalling = ii.PercentRSBuyFalling,
                                         PercentRSBuySell = ii.PercentRSBuySell,
                                         PercentRsRisingFalling = ii.PercentRsRisingFalling,
                                         PercentRsRisingSell = ii.PercentRsRisingSell,
                                         PercentPositiveTrendFalling = ii.PercentPositiveTrendFalling,
                                         PercentPositiveTrendSell = ii.PercentPositiveTrendSell,
                                         PercentAbove30EmaFalling = ii.PercentAbove30EmaFalling,
                                         PercentAbove30EmaSell = ii.PercentAbove30EmaSell,
                                         PercentAbove10EmaFalling = ii.PercentAbove10EmaFalling,
                                         PercentAbove10EmaSell = ii.PercentAbove10EmaSell,
                                         HighLowIndexRising = ii.HighLowIndexRising,
                                         HighLowIndexBuy = ii.HighLowIndexBuy,
                                         HighLowIndexFalling = ii.HighLowIndexFalling,
                                         HighLowIndexSell = ii.HighLowIndexSell,
                                         NewEvents = ii.NewEvents,
                                         Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "RSX Buy " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "RS Buy Buy " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "PT Buy " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentOf10Gt30) == (int)IndexEvents.PercentOf10Gt30 ? "Percent of 10 (Above 30) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentOf30Gt30) == (int)IndexEvents.PercentOf30Gt30 ? "Percent of 30 (Above 30) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.HighLowGt30) == (int)IndexEvents.HighLowGt30 ? "High-Low (Above 30) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "RSX Sell " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "RS Buy Sell " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "PT Sell " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentOf10Lt70) == (int)IndexEvents.PercentOf10Lt70 ? "Percent of 10 (Below 70) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.PercentOf30Lt70) == (int)IndexEvents.PercentOf30Lt70 ? "Percent of 30 (Below 70) " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.HighLowLt70) == (int)IndexEvents.HighLowLt70 ? "High-Low (Below 70) " : "")
                                     }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the index data", ex);
            }
            return indices;
        }



        public async Task<IEnumerable<ShareSummaryDTO>> GetShareValuesAsync(MarketSummaryDTO marketSummaryDTO)
        {
            IEnumerable<ShareSummaryDTO> shares = new List<ShareSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    DateTime day = marketSummaryDTO.Day;
                    shares = await (from s in db.Shares
                                    from si in db.ShareIndicators.Where(r => r.ShareId == s.Id && r.Day == day).DefaultIfEmpty()
                                    from rs in db.ShareRSIValues.Where(r => r.ShareId == s.Id && r.Day == day && r.RelativeTo == RelativeToEnum.Market).DefaultIfEmpty()
                                    from prs in db.ShareRSIValues.Where(r => r.ShareId == s.Id && r.Day == day && r.RelativeTo == RelativeToEnum.Sector).DefaultIfEmpty()
                                    from q in db.EodPrices.Where(r => r.ShareId == s.Id && r.Day == day).DefaultIfEmpty()
                                    from idx in db.Indices.Where(r => r.ExchangeCode == s.ExchangeCode && r.ExchangeSubCode == s.ExchangeSubCode && r.SuperSector == s.SuperSector).DefaultIfEmpty()
                                    from ii in db.IndexIndicators.Where(r => r.IndexId == idx.Id && r.Day == day)
                                    where s.ExchangeCode == marketSummaryDTO.ExchangeCode
                                        && s.ExchangeSubCode == marketSummaryDTO.ExchangeSubCode
                                        && (s.SuperSector == marketSummaryDTO.SuperSector || marketSummaryDTO.SuperSector == null)
                                    orderby s.Tidm
                                    select new ShareSummaryDTO()
                                    {
                                        Id = s.Id,
                                        Tidm = s.Tidm,
                                        Name = s.Name,
                                        ExchangeCode = s.ExchangeCode,
                                        ExchangeSubCode = s.ExchangeSubCode,
                                        SuperSector = s.SuperSector,
                                        MarketCapMillions = s.MarketCapMillions,
                                        Close = q.Close,
                                        RsValue = rs.Value,
                                        PeerRsValue = prs.Value,
                                        Ema10 = si.Ema10 ?? 0d,
                                        Ema30 = si.Ema30 ?? 0d,
                                        ClosedAboveEma10 = si.ClosedAboveEma10 ?? false,
                                        ClosedAboveEma30 = si.ClosedAboveEma30 ?? false,
                                        Rising = si.Rising ?? false,
                                        DoubleTop = si.DoubleTop ?? false,
                                        TripleTop = si.TripleTop ?? false,
                                        RsRising = si.RsRising ?? false,
                                        RsBuy = si.RsBuy ?? false,
                                        PeerRsRising = si.PeerRsRising ?? false,
                                        PeerRsBuy = si.PeerRsBuy ?? false,
                                        Falling = si.Falling ?? false,
                                        DoubleBottom = si.DoubleBottom ?? false,
                                        TripleBottom = si.TripleBottom ?? false,
                                        RsFalling = si.RsFalling ?? false,
                                        RsSell = si.RsSell ?? false,
                                        PeerRsFalling = si.PeerRsFalling ?? false,
                                        PeerRsSell = si.PeerRsSell ?? false,
                                        AboveBullSupport = si.AboveBullSupport,
                                        WeeklyMomentum = si.WeeklyMomentum,
                                        MomentumFalling = si.MomentumFalling,
                                        MomentumRising = si.MomentumRising,
                                        DividendYearsPaid = (int?)s.DividendYearsPaid,
                                        ForecastYield = s.ForecastYield,
                                        NewEvents = si.NewEvents,
                                        Score = 0 + (si.Rising == true ? 1 : 0)
                                                  + (si.Falling == true ? -1 : 0)
                                                  + (si.DoubleTop == true ? 1 : 0)
                                                  + (si.DoubleBottom == true ? -1 : 0)
                                                  + (si.TripleTop == true ? 1 : 0)
                                                  + (si.TripleBottom == true ? -1 : 0)
                                                  + (si.RsRising == true ? 1 : 0)
                                                  + (si.RsFalling == true ? -1 : 0)
                                                  + (si.RsBuy == true ? 1 : 0)
                                                  + (si.RsSell == true ? -1 : 0)
                                                  + (si.PeerRsRising == true ? 1 : 0)
                                                  + (si.PeerRsFalling == true ? -1 : 0)
                                                  + (si.PeerRsBuy == true ? 1 : 0)
                                                  + (si.PeerRsSell == true ? -1 : 0)
                                                  + (si.ClosedAboveEma10 == true ? 1 : 0)
                                                  + (si.ClosedAboveEma30 == true ? 1 : 0)
                                                  + (si.AboveBullSupport == true ? 1 : 0),
                                        Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "Bull Confirmed (Below 30%)" : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                            + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "Bear Confirmed (Above 70%)" : "")
                                            + ((si.NewEvents & (int)ShareEvents.MomentumGonePositive) == (int)ShareEvents.MomentumGonePositive ? "Moment Positive" : "")
                                            + ((si.NewEvents & (int)ShareEvents.MomentumGoneNegative) == (int)ShareEvents.MomentumGoneNegative ? "Moment Negative" : "")
                                    }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share values data", ex);
            }
            return shares;

        }

        public async Task<IEnumerable<ShareSummaryDTO>> GetEventFilteredSharesAsync(ShareEvents eventFilter, DateTime day, string exchangeCode)
        {
            IEnumerable<ShareSummaryDTO> shares = new List<ShareSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    if (exchangeCode == "<All>")
                    {
                        shares = await (from si in db.ShareIndicators
                                        join s in db.Shares on si.ShareId equals s.Id
                                        from rs in db.ShareRSIValues.Where(r => r.ShareId == si.ShareId && r.Day == si.Day && r.RelativeTo == RelativeToEnum.Market).DefaultIfEmpty()    // on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Market } equals new { rs.ShareId, rs.Day, rs.RelativeTo }
                                        from prs in db.ShareRSIValues.Where(r => r.ShareId == si.ShareId && r.Day == si.Day && r.RelativeTo == RelativeToEnum.Sector).DefaultIfEmpty()   // on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Sector } equals new { prs.ShareId, prs.Day, prs.RelativeTo }
                                        from q in db.EodPrices.Where(r => r.ShareId == si.ShareId && r.Day == si.Day).DefaultIfEmpty()                                                   //  on new { si.ShareId, si.Day } equals new { q.ShareId, q.Day }
                                        from idx in db.Indices.Where(r => r.ExchangeCode == s.ExchangeCode && r.ExchangeSubCode == s.ExchangeSubCode && r.SuperSector == s.SuperSector).DefaultIfEmpty()
                                        from ii in db.IndexIndicators.Where(r => r.IndexId == idx.Id && r.Day == day)
                                        where si.Day == day && (si.NewEvents & (int)eventFilter) != 0
                                        orderby s.Tidm
                                        select new ShareSummaryDTO()
                                        {
                                            Id = s.Id,
                                            Tidm = s.Tidm,
                                            Name = s.Name,
                                            ExchangeCode = s.ExchangeCode,
                                            ExchangeSubCode = s.ExchangeSubCode,
                                            SuperSector = s.SuperSector,
                                            MarketCapMillions = s.MarketCapMillions,
                                            Close = q.Close,
                                            RsValue = rs.Value,
                                            PeerRsValue = prs.Value,
                                            Ema10 = si.Ema10 ?? 0d,
                                            Ema30 = si.Ema30 ?? 0d,
                                            ClosedAboveEma10 = si.ClosedAboveEma10 ?? false,
                                            ClosedAboveEma30 = si.ClosedAboveEma30 ?? false,
                                            Rising = si.Rising ?? false,
                                            DoubleTop = si.DoubleTop ?? false,
                                            TripleTop = si.TripleTop ?? false,
                                            RsRising = si.RsRising ?? false,
                                            RsBuy = si.RsBuy ?? false,
                                            PeerRsRising = si.PeerRsRising ?? false,
                                            PeerRsBuy = si.PeerRsBuy ?? false,
                                            Falling = si.Falling ?? false,
                                            DoubleBottom = si.DoubleBottom ?? false,
                                            TripleBottom = si.TripleBottom ?? false,
                                            RsFalling = si.RsFalling ?? false,
                                            RsSell = si.RsSell ?? false,
                                            PeerRsFalling = si.PeerRsFalling ?? false,
                                            PeerRsSell = si.PeerRsSell ?? false,
                                            AboveBullSupport = si.AboveBullSupport,
                                            WeeklyMomentum = si.WeeklyMomentum,
                                            MomentumFalling = si.MomentumFalling,
                                            MomentumRising = si.MomentumRising,
                                            DividendYearsPaid = (int?)s.DividendYearsPaid,
                                            ForecastYield = s.ForecastYield,
                                            NewEvents = si.NewEvents,
                                            Score = 0 + (ii.BullishPercentRising == true ? 1 : 0)
                                                         + (ii.BullishPercentFalling == true ? -1 : 0)
                                                         + (ii.BullishPercentDoubleTop == true ? 1 : 0)
                                                         + (ii.BullishPercentDoubleBottom == true ? -1 : 0)
                                                         + (ii.RsRising == true ? 1 : 0)
                                                         + (ii.RsFalling == true ? -1 : 0)
                                                         + (ii.RsBuy == true ? 1 : 0)
                                                         + (ii.RsSell == true ? -1 : 0)
                                                         + (ii.PercentRSBuyRising == true ? 1 : 0)
                                                         + (ii.PercentRSBuyFalling == true ? -1 : 0)
                                                         + (ii.PercentRSBuyBuy == true ? 1 : 0)
                                                         + (ii.PercentRSBuySell == true ? -1 : 0)
                                                         + (ii.PercentRsRisingRising == true ? 1 : 0)
                                                         + (ii.PercentRsRisingFalling == true ? -1 : 0)
                                                         + (ii.PercentRsRisingBuy == true ? 1 : 0)
                                                         + (ii.PercentRsRisingSell == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendRising == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendFalling == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendBuy == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendSell == true ? -1 : 0) + (si.Rising == true ? 1 : 0)
                                                      + (si.Falling == true ? -1 : 0)
                                                      + (si.DoubleTop == true ? 1 : 0)
                                                      + (si.DoubleBottom == true ? -1 : 0)
                                                      + (si.TripleTop == true ? 1 : 0)
                                                      + (si.TripleBottom == true ? -1 : 0)
                                                      + (si.RsRising == true ? 1 : 0)
                                                      + (si.RsFalling == true ? -1 : 0)
                                                      + (si.RsBuy == true ? 1 : 0)
                                                      + (si.RsSell == true ? -1 : 0)
                                                      + (si.PeerRsRising == true ? 1 : 0)
                                                      + (si.PeerRsFalling == true ? -1 : 0)
                                                      + (si.PeerRsBuy == true ? 1 : 0)
                                                      + (si.PeerRsSell == true ? -1 : 0)
                                                      + (si.ClosedAboveEma10 == true ? 1 : 0)
                                                      + (si.ClosedAboveEma30 == true ? 1 : 0)
                                                      + (si.AboveBullSupport == true ? 1 : 0),
                                            Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsBuy) == (int)IndexEvents.RsBuy ? "RS Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "RSX [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "RS Buy [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "PT [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsSell) == (int)IndexEvents.RsSell ? "RS [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "RSX [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "RS Buy [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "PT [Sell] " : "")
                                                + ((si.NewEvents & (int)ShareEvents.MomentumGonePositive) == (int)ShareEvents.MomentumGonePositive ? "Moment Positive" : "")
                                                + ((si.NewEvents & (int)ShareEvents.MomentumGoneNegative) == (int)ShareEvents.MomentumGoneNegative ? "Moment Negative" : "")
                                        }).ToListAsync();
                    }
                    else
                    {
                        shares = await (from si in db.ShareIndicators
                                        join s in db.Shares on si.ShareId equals s.Id
                                        from rs in db.ShareRSIValues.Where(r => r.ShareId == si.ShareId && r.Day == si.Day && r.RelativeTo == RelativeToEnum.Market).DefaultIfEmpty()    // on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Market } equals new { rs.ShareId, rs.Day, rs.RelativeTo }
                                        from prs in db.ShareRSIValues.Where(r => r.ShareId == si.ShareId && r.Day == si.Day && r.RelativeTo == RelativeToEnum.Sector).DefaultIfEmpty()   // on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Sector } equals new { prs.ShareId, prs.Day, prs.RelativeTo }
                                        from q in db.EodPrices.Where(r => r.ShareId == si.ShareId && r.Day == si.Day).DefaultIfEmpty()                                                   //  on new { si.ShareId, si.Day } equals new { q.ShareId, q.Day }
                                        from idx in db.Indices.Where(r => r.ExchangeCode == s.ExchangeCode && r.ExchangeSubCode == s.ExchangeSubCode && r.SuperSector == s.SuperSector).DefaultIfEmpty()
                                        from ii in db.IndexIndicators.Where(r => r.IndexId == idx.Id && r.Day == day)
                                        where s.ExchangeCode == exchangeCode && si.Day == day && (si.NewEvents & (int)eventFilter) != 0
                                        orderby s.Tidm
                                        select new ShareSummaryDTO()
                                        {
                                            Id = s.Id,
                                            Tidm = s.Tidm,
                                            Name = s.Name,
                                            ExchangeCode = s.ExchangeCode,
                                            ExchangeSubCode = s.ExchangeSubCode,
                                            SuperSector = s.SuperSector,
                                            MarketCapMillions = s.MarketCapMillions,
                                            Close = q.Close,
                                            RsValue = rs.Value,
                                            PeerRsValue = prs.Value,
                                            Ema10 = si.Ema10 ?? 0d,
                                            Ema30 = si.Ema30 ?? 0d,
                                            ClosedAboveEma10 = si.ClosedAboveEma10 ?? false,
                                            ClosedAboveEma30 = si.ClosedAboveEma30 ?? false,
                                            Rising = si.Rising ?? false,
                                            DoubleTop = si.DoubleTop ?? false,
                                            TripleTop = si.TripleTop ?? false,
                                            RsRising = si.RsRising ?? false,
                                            RsBuy = si.RsBuy ?? false,
                                            PeerRsRising = si.PeerRsRising ?? false,
                                            PeerRsBuy = si.PeerRsBuy ?? false,
                                            Falling = si.Falling ?? false,
                                            DoubleBottom = si.DoubleBottom ?? false,
                                            TripleBottom = si.TripleBottom ?? false,
                                            RsFalling = si.RsFalling ?? false,
                                            RsSell = si.RsSell ?? false,
                                            PeerRsFalling = si.PeerRsFalling ?? false,
                                            PeerRsSell = si.PeerRsSell ?? false,
                                            AboveBullSupport = si.AboveBullSupport,
                                            WeeklyMomentum = si.WeeklyMomentum,
                                            MomentumFalling = si.MomentumFalling,
                                            MomentumRising = si.MomentumRising,
                                            DividendYearsPaid = (int?)s.DividendYearsPaid,
                                            ForecastYield = s.ForecastYield,
                                            NewEvents = si.NewEvents,
                                            Score = 0 + (ii.BullishPercentRising == true ? 1 : 0)
                                                         + (ii.BullishPercentFalling == true ? -1 : 0)
                                                         + (ii.BullishPercentDoubleTop == true ? 1 : 0)
                                                         + (ii.BullishPercentDoubleBottom == true ? -1 : 0)
                                                         + (ii.RsRising == true ? 1 : 0)
                                                         + (ii.RsFalling == true ? -1 : 0)
                                                         + (ii.RsBuy == true ? 1 : 0)
                                                         + (ii.RsSell == true ? -1 : 0)
                                                         + (ii.PercentRSBuyRising == true ? 1 : 0)
                                                         + (ii.PercentRSBuyFalling == true ? -1 : 0)
                                                         + (ii.PercentRSBuyBuy == true ? 1 : 0)
                                                         + (ii.PercentRSBuySell == true ? -1 : 0)
                                                         + (ii.PercentRsRisingRising == true ? 1 : 0)
                                                         + (ii.PercentRsRisingFalling == true ? -1 : 0)
                                                         + (ii.PercentRsRisingBuy == true ? 1 : 0)
                                                         + (ii.PercentRsRisingSell == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendRising == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendFalling == true ? -1 : 0)
                                                         + (ii.PercentPositiveTrendBuy == true ? 1 : 0)
                                                         + (ii.PercentPositiveTrendSell == true ? -1 : 0) + (si.Rising == true ? 1 : 0)
                                                      + (si.Falling == true ? -1 : 0)
                                                      + (si.DoubleTop == true ? 1 : 0)
                                                      + (si.DoubleBottom == true ? -1 : 0)
                                                      + (si.TripleTop == true ? 1 : 0)
                                                      + (si.TripleBottom == true ? -1 : 0)
                                                      + (si.RsRising == true ? 1 : 0)
                                                      + (si.RsFalling == true ? -1 : 0)
                                                      + (si.RsBuy == true ? 1 : 0)
                                                      + (si.RsSell == true ? -1 : 0)
                                                      + (si.PeerRsRising == true ? 1 : 0)
                                                      + (si.PeerRsFalling == true ? -1 : 0)
                                                      + (si.PeerRsBuy == true ? 1 : 0)
                                                      + (si.PeerRsSell == true ? -1 : 0)
                                                      + (si.ClosedAboveEma10 == true ? 1 : 0)
                                                      + (si.ClosedAboveEma30 == true ? 1 : 0)
                                                      + (si.AboveBullSupport == true ? 1 : 0),
                                            Notices = ((ii.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Bull Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Bull Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsBuy) == (int)IndexEvents.RsBuy ? "RS Buy " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "RSX [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "RS Buy [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "PT [Buy] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Bear Alert " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Bear Confirmed " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%) " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.RsSell) == (int)IndexEvents.RsSell ? "RS [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "RSX [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "RS Buy [Sell] " : "")
                                                + ((ii.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "PT [Sell] " : "")
                                                + ((si.NewEvents & (int)ShareEvents.MomentumGonePositive) == (int)ShareEvents.MomentumGonePositive ? "Moment Positive" : "")
                                                + ((si.NewEvents & (int)ShareEvents.MomentumGoneNegative) == (int)ShareEvents.MomentumGoneNegative ? "Moment Negative" : "")
                                        }).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share values data", ex);
            }
            return shares;

        }


        public async Task<IEnumerable<string>> GetExchangeCodesAsync()
        {
            IEnumerable<string> exchangeCodes = new List<string>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    exchangeCodes = await db.Indices.Select(s => s.ExchangeCode).Distinct().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share values data", ex);
            }
            return exchangeCodes;

        }


        public async Task<IEnumerable<PortfolioShareSummaryDTO>> GetPortfolioValuesAsync(Portfolio portfolio, DateTime day)
        {
            IEnumerable<PortfolioShareSummaryDTO> shares = new List<PortfolioShareSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    shares = await (from ps in db.PortfolioShares
                                    join s in db.Shares on ps.ShareId equals s.Id
                                    from si in db.ShareIndicators.Where(r => r.ShareId == ps.ShareId && r.Day == day).DefaultIfEmpty()
                                    from rs in db.ShareRSIValues.Where(r => r.ShareId == ps.ShareId && r.Day == day && r.RelativeTo == RelativeToEnum.Market).DefaultIfEmpty()
                                    from prs in db.ShareRSIValues.Where(r => r.ShareId == ps.ShareId && r.Day == day && r.RelativeTo == RelativeToEnum.Sector).DefaultIfEmpty()
                                    from q in db.EodPrices.Where(r => r.ShareId == ps.ShareId && r.Day == day).DefaultIfEmpty()
                                    from mkt in db.Indices.Where(r => r.ExchangeCode == s.ExchangeCode && r.ExchangeSubCode == s.ExchangeSubCode && r.SuperSector == null).DefaultIfEmpty()
                                    from mktv in db.IndexValues.Where(r => r.IndexId == mkt.Id && r.Day == day).DefaultIfEmpty()
                                    from mkti in db.IndexIndicators.Where(r => r.IndexId == mkt.Id && r.Day == day).DefaultIfEmpty()
                                    from sec in db.Indices.Where(r => r.ExchangeCode == s.ExchangeCode && r.ExchangeSubCode == s.ExchangeSubCode && r.SuperSector == s.SuperSector).DefaultIfEmpty()
                                    from secv in db.IndexValues.Where(r => r.IndexId == sec.Id && r.Day == day).DefaultIfEmpty()
                                    from seci in db.IndexIndicators.Where(r => r.IndexId == sec.Id && r.Day == day).DefaultIfEmpty()
                                    from secrs in db.IndexRSIValues.Where(r => r.IndexId == sec.Id && r.Day == day).DefaultIfEmpty()
                                    where ps.PortfolioId == portfolio.Id
                                    orderby s.Tidm
                                    select new PortfolioShareSummaryDTO()
                                    {
                                        Id = s.Id,
                                        Tidm = s.Tidm,
                                        Name = s.Name,
                                        ExchangeCode = s.ExchangeCode,
                                        ExchangeSubCode = s.ExchangeSubCode,
                                        SuperSector = s.SuperSector,
                                        MarketCapMillions = s.MarketCapMillions,
                                        Close = q.Close,
                                        RsValue = rs.Value,
                                        PeerRsValue = prs.Value,
                                        Ema10 = si.Ema10 ?? 0d,
                                        Ema30 = si.Ema30 ?? 0d,
                                        ClosedAboveEma10 = si.ClosedAboveEma10 ?? false,
                                        ClosedAboveEma30 = si.ClosedAboveEma30 ?? false,
                                        Rising = si.Rising ?? false,
                                        DoubleTop = si.DoubleTop ?? false,
                                        TripleTop = si.TripleTop ?? false,
                                        RsRising = si.RsRising ?? false,
                                        RsBuy = si.RsBuy ?? false,
                                        PeerRsRising = si.PeerRsRising ?? false,
                                        PeerRsBuy = si.PeerRsBuy ?? false,
                                        Falling = si.Falling ?? false,
                                        DoubleBottom = si.DoubleBottom ?? false,
                                        TripleBottom = si.TripleBottom ?? false,
                                        RsFalling = si.RsFalling ?? false,
                                        RsSell = si.RsSell ?? false,
                                        PeerRsFalling = si.PeerRsFalling ?? false,
                                        PeerRsSell = si.PeerRsSell ?? false,
                                        AboveBullSupport = si.AboveBullSupport,
                                        WeeklyMomentum = si.WeeklyMomentum,
                                        MomentumFalling = si.MomentumFalling,
                                        MomentumRising = si.MomentumRising,
                                        ForecastYield = s.ForecastYield,
                                        DividendYearsPaid = (int?)s.DividendYearsPaid,
                                        NewEvents = si.NewEvents,
                                        Score = 0
                                                + (mkti.PercentRSBuyBuy == true ? 1 : 0)
                                                + (mkti.PercentRSBuySell == true ? -1 : 0)
                                                + (mkti.PercentRsRisingBuy == true ? 1 : 0)
                                                + (mkti.PercentRsRisingSell == true ? -1 : 0)
                                                + (mkti.PercentPositiveTrendBuy == true ? 1 : 0)
                                                + (mkti.PercentPositiveTrendSell == true ? -1 : 0)
                                                + (mkti.BullishPercentDoubleTop == true ? 1 : 0)
                                                + (mkti.BullishPercentDoubleBottom == true ? -1 : 0)
                                                + (mkti.PercentAbove30EmaBuy == true ? 1 : 0)
                                                + (mkti.PercentAbove30EmaSell == true ? -1 : 0)
                                                + (mkti.PercentAbove10EmaBuy == true ? 1 : 0)
                                                + (mkti.PercentAbove10EmaSell == true ? -1 : 0)
                                                + (mkti.HighLowIndexBuy == true ? 1 : 0)
                                                + (mkti.HighLowIndexSell == true ? -1 : 0)
                                                + (seci.BullishPercentDoubleTop == true ? 1 : 0)
                                                + (seci.BullishPercentDoubleBottom == true ? -1 : 0)
                                                + (seci.RsBuy == true ? 1 : 0)
                                                + (seci.RsSell == true ? -1 : 0)
                                                + (seci.PercentRSBuyBuy == true ? 1 : 0)
                                                + (seci.PercentRSBuySell == true ? -1 : 0)
                                                + (seci.PercentRsRisingBuy == true ? 1 : 0)
                                                + (seci.PercentRsRisingSell == true ? -1 : 0)
                                                + (seci.PercentPositiveTrendBuy == true ? 1 : 0)
                                                + (seci.PercentPositiveTrendSell == true ? -1 : 0)
                                                  + (si.Rising == true ? 1 : 0)
                                                  + (si.Falling == true ? -1 : 0)
                                                  + (si.DoubleTop == true ? 1 : 0)
                                                  + (si.DoubleBottom == true ? -1 : 0)
                                                  + (si.TripleTop == true ? 1 : 0)
                                                  + (si.TripleBottom == true ? -1 : 0)
                                                  + (si.RsRising == true ? 1 : 0)
                                                  + (si.RsFalling == true ? -1 : 0)
                                                  + (si.RsBuy == true ? 1 : 0)
                                                  + (si.RsSell == true ? -1 : 0)
                                                  + (si.PeerRsRising == true ? 1 : 0)
                                                  + (si.PeerRsFalling == true ? -1 : 0)
                                                  + (si.PeerRsBuy == true ? 1 : 0)
                                                  + (si.PeerRsSell == true ? -1 : 0)
                                                  + (si.ClosedAboveEma10 == true ? 1 : 0)
                                                  + (si.ClosedAboveEma30 == true ? 1 : 0)
                                                  + (si.AboveBullSupport == true ? 1 : 0),
                                        #region Market indicators ...
                                        MarketIndexId = mktv.IndexId,
                                        MarketBullishPercent = mktv.BullishPercent,
                                        MarketPercentAboveEma10 = mktv.PercentAboveEma10,
                                        MarketPercentAboveEma30 = mktv.PercentAboveEma30,
                                        MarketPercentRsBuy = mktv.PercentRsBuy,
                                        MarketPercentRsRising = mktv.PercentRsRising,
                                        MarketPercentPositiveTrend = mktv.PercentPositiveTrend,
                                        MarketHighLowIndexValue = mktv.HighLowEma10,
                                        MarketRsRising = mkti.RsRising,
                                        MarketRsBuy = mkti.RsBuy,
                                        MarketRsFalling = mkti.RsFalling,
                                        MarketRsSell = mkti.RsSell,
                                        MarketBullishPercentRising = mkti.BullishPercentRising,
                                        MarketPercentRsBuyRising = mkti.PercentRSBuyRising,
                                        MarketPercentRsRisingRising = mkti.PercentRsRisingRising,
                                        MarketPercentPositiveTrendRising = mkti.PercentPositiveTrendRising,
                                        MarketPercentAbove30EmaRising = mkti.PercentAbove30EmaRising,
                                        MarketPercentAbove10EmaRising = mkti.PercentAbove10EmaRising,
                                        MarketBullishPercentFalling = mkti.BullishPercentFalling,
                                        MarketPercentRsBuyFalling = mkti.PercentRSBuyFalling,
                                        MarketPercentRsRisingFalling = mkti.PercentRsRisingFalling,
                                        MarketPercentPositiveTrendFalling = mkti.PercentPositiveTrendFalling,
                                        MarketPercentAbove30EmaFalling = mkti.PercentAbove30EmaFalling,
                                        MarketPercentAbove10EmaFalling = mkti.PercentAbove10EmaFalling,
                                        MarketHighLowIndexRising = mkti.HighLowIndexRising,
                                        MarketHighLowIndexFalling = mkti.HighLowIndexFalling,
                                        MarketBullishPercentBuy = mkti.BullishPercentDoubleTop,
                                        MarketPercentRsBuyBuy = mkti.PercentRSBuyBuy,
                                        MarketPercentRsRisingBuy = mkti.PercentRsRisingBuy,
                                        MarketPercentPositiveTrendBuy = mkti.PercentPositiveTrendBuy,
                                        MarketPercentAbove30EmaBuy = mkti.PercentAbove30EmaBuy,
                                        MarketPercentAbove10EmaBuy = mkti.PercentAbove10EmaBuy,
                                        MarketBullishPercentSell = mkti.BullishPercentDoubleBottom,
                                        MarketPercentRsBuySell = mkti.PercentRSBuySell,
                                        MarketPercentRsRisingSell = mkti.PercentRsRisingSell,
                                        MarketPercentPositiveTrendSell = mkti.PercentPositiveTrendSell,
                                        MarketPercentAbove30EmaSell = mkti.PercentAbove30EmaSell,
                                        MarketPercentAbove10EmaSell = mkti.PercentAbove10EmaSell,
                                        MarketHighLowIndexBuy = mkti.HighLowIndexBuy,
                                        MarketHighLowIndexSell = mkti.HighLowIndexSell,
                                        MarketNewEvents = mkti.NewEvents,
                                        #endregion
                                        #region Sector indicators ...
                                        SectorIndexId = secv.IndexId,
                                        SectorBullishPercent = secv.BullishPercent,
                                        SectorPercentRsBuy = secv.PercentRsBuy,
                                        SectorPercentRsRising = secv.PercentRsRising,
                                        SectorPercentPositiveTrend = secv.PercentPositiveTrend,
                                        SectorRsValue = secrs.Value,
                                        SectorRsRising = seci.RsRising,
                                        SectorRsBuy = seci.RsBuy,
                                        SectorRsFalling = seci.RsFalling,
                                        SectorRsSell = seci.RsSell,
                                        SectorBullishPercentRising = seci.BullishPercentRising,
                                        SectorPercentRsBuyRising = seci.PercentRSBuyRising,
                                        SectorPercentRsRisingRising = seci.PercentRsRisingRising,
                                        SectorPercentPositiveTrendRising = seci.PercentPositiveTrendRising,
                                        SectorBullishPercentFalling = seci.BullishPercentFalling,
                                        SectorPercentRsBuyFalling = seci.PercentRSBuyFalling,
                                        SectorPercentRsRisingFalling = seci.PercentRsRisingFalling,
                                        SectorPercentPositiveTrendFalling = seci.PercentPositiveTrendFalling,
                                        SectorBullishPercentBuy = seci.BullishPercentDoubleTop,
                                        SectorPercentRsBuyBuy = seci.PercentRSBuyBuy,
                                        SectorPercentRsRisingBuy = seci.PercentRsRisingBuy,
                                        SectorPercentPositiveTrendBuy = seci.PercentPositiveTrendBuy,
                                        SectorBullishPercentSell = seci.BullishPercentDoubleBottom,
                                        SectorPercentRsBuySell = seci.PercentRSBuySell,
                                        SectorPercentRsRisingSell = seci.PercentRsRisingSell,
                                        SectorPercentPositiveTrendSell = seci.PercentPositiveTrendSell,
                                        SectorNewEvents = seci.NewEvents,
                                        #endregion
                                        Notices = ((mkti.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Market Bull Alert " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Market Bull Confirmed " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%)" : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "Market RSX Buy " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "Market RS Buy Buy " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "Market PT Buy " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentOf10Gt30) == (int)IndexEvents.PercentOf10Gt30 ? "Market Percent of 10 (Above 30) " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentOf30Gt30) == (int)IndexEvents.PercentOf30Gt30 ? "Market Percent of 30 (Above 30) " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.HighLowGt30) == (int)IndexEvents.HighLowGt30 ? "Market High-Low (Above 30) " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Market Bear Alert " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Market Bear Confirmed " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%)" : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "Market RSX Sell " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "Market RS Buy Sell " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "Market PT Sell " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentOf10Lt70) == (int)IndexEvents.PercentOf10Lt70 ? "Market Percent of 10 (Below 70) " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.PercentOf30Lt70) == (int)IndexEvents.PercentOf30Lt70 ? "Market Percent of 30 (Below 70) " : "")
                                            + ((mkti.NewEvents & (int)IndexEvents.HighLowLt70) == (int)IndexEvents.HighLowLt70 ? "Market High-Low (Below 70) " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BullAlert) == (int)IndexEvents.BullAlert ? "Sector Bull Alert " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BullConfirmed) == (int)IndexEvents.BullConfirmed ? "Sector Bull Confirmed " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BullConfirmedLt30) == (int)IndexEvents.BullConfirmedLt30 ? "(Below 30%)" : "")
                                            + ((seci.NewEvents & (int)IndexEvents.RsBuy) == (int)IndexEvents.RsBuy ? "Sector RS Buy " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentRsXBuy) == (int)IndexEvents.PercentRsXBuy ? "Sector RSX Buy " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentRsBuyBuy) == (int)IndexEvents.PercentRsBuyBuy ? "Sector RS Buy Buy " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentPtBuy) == (int)IndexEvents.PercentPtBuy ? "Sector PT Buy " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BearAlert) == (int)IndexEvents.BearAlert ? "Sector Bear Alert " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BearConfirmed) == (int)IndexEvents.BearConfirmed ? "Sector Bear Confirmed " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.BearConfirmedGt70) == (int)IndexEvents.BearConfirmedGt70 ? "(Above 70%)" : "")
                                            + ((seci.NewEvents & (int)IndexEvents.RsSell) == (int)IndexEvents.RsSell ? "Sector RS Sell " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentRsXSell) == (int)IndexEvents.PercentRsXSell ? "Sector RSX Sell " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentRsBuySell) == (int)IndexEvents.PercentRsBuySell ? "Sector RS Buy Sell " : "")
                                            + ((seci.NewEvents & (int)IndexEvents.PercentPtSell) == (int)IndexEvents.PercentPtSell ? "Sector PT Sell " : "")
                                            + ((si.NewEvents & (int)ShareEvents.MomentumGonePositive) == (int)ShareEvents.MomentumGonePositive ? "Moment Positive" : "")
                                            + ((si.NewEvents & (int)ShareEvents.MomentumGoneNegative) == (int)ShareEvents.MomentumGoneNegative ? "Moment Negative" : "")
                                    }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share values data", ex);
            }
            return shares;

        }

        public async Task<bool> UpdatePortfolioAsync(Portfolio portfolio)
        {
            bool result = false;
            try
            {
                using (var db = new PnFDataContext())
                {
                    db.Portfolios.Update(portfolio);
                    result = (await db.SaveChangesAsync() > 0);
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, $"An error occurred updating portfolio '{portfolio.Name}'", ex);
            }
            return result;
        }

        public async Task<bool> DeletePortfolioShareAsync(PortfolioShare portfolioShare)
        {
            bool result = false;
            try
            {
                using (var db = new PnFDataContext())
                {
                    db.PortfolioShares.Remove(portfolioShare);
                    result = (await db.SaveChangesAsync() > 0);
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred deleting the portfolio share", ex);
            }
            return result;
        }


        public async Task<StdDevResult> GetStandardDeviationAsync(Guid shareId, int days)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT TOP 1 ");
            sb.Append($"AVG([AdjustedClose]) OVER(PARTITION BY [ShareId] ORDER BY[ShareId], [Day] ROWS BETWEEN {days - 1} PRECEDING AND CURRENT ROW) [Mean] ");
            sb.Append($", STDEV([AdjustedClose]) OVER(PARTITION BY [ShareId] ORDER BY [ShareId], [Day] ROWS BETWEEN {days - 1} PRECEDING AND CURRENT ROW) [StdDev] ");
            sb.Append($"FROM EodPrices WHERE[ShareId] = '{shareId}' ORDER BY[ShareId], [Day] DESC");

            StdDevResult result = null;
            try
            {
                using (var db = new PnFDataContext())
                {
                    result = await db.StdDevResults.FromSqlRaw(sb.ToString()).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred getting the trading band.", ex);
            }
            return result;

        }
    }
}
