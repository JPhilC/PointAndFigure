using Microsoft.EntityFrameworkCore;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.DTOs;
using PnFDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
                                        chart = await db.PnFCharts
                                            .Include(cc => cc.Columns).ThenInclude(cb => cb.Boxes)
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
                                        chart = await db.PnFCharts
                                            .Include(cc => cc.Columns).ThenInclude(cb => cb.Boxes)
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

        public async Task<IEnumerable<MarketSummaryDTO>> GetMarketValuesAsync(DateTime day)
        {
            IEnumerable<MarketSummaryDTO> indices = new List<MarketSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    indices = await (from iv in db.IndexValues
                                     join i in db.Indices on iv.IndexId equals i.Id
                                     join ii in db.IndexIndicators on new { iv.IndexId, iv.Day } equals new { ii.IndexId, ii.Day }
                                     where iv.Day == day
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
                                         Contributors = iv.Contributors,
                                         BullishPercent = iv.BullishPercent,
                                         PercentAboveEma10 = iv.PercentAboveEma10,
                                         PercentAboveEma30 = iv.PercentAboveEma30,
                                         PercentRsBuy = iv.PercentRsBuy,
                                         PercentRsRising = iv.PercentRsRising,
                                         PercentPositiveTrend = iv.PercentPositiveTrend,
                                         Rising = ii.Rising,
                                         Buy = ii.Buy,
                                         RsRising = ii.RsRising,
                                         RsBuy = ii.RsBuy,
                                         Falling = ii.Falling,
                                         Sell = ii.Sell,
                                         RsFalling = ii.RsFalling,
                                         RsSell = ii.RsSell,
                                         BullishPercentRising = ii.BullishPercentRising,
                                         PercentRSBuyRising = ii.PercentRSBuyRising,
                                         PercentRsRisingRising = ii.PercentRsRisingRising,
                                         PercentPositiveTrendRising = ii.PercentPositiveTrendRising,
                                         PercentAbove30EmaRising = ii.PercentAbove30EmaRising,
                                         PercentAbove10EmaRising = ii.PercentAbove10EmaRising,
                                         BullishPercentFalling = ii.BullishPercentFalling,
                                         PercentRSBuyFalling = ii.PercentRSBuyFalling,
                                         PercentRsRisingFalling = ii.PercentRsRisingFalling,
                                         PercentPositiveTrendFalling = ii.PercentPositiveTrendFalling,
                                         PercentAbove30EmaFalling = ii.PercentAbove30EmaFalling,
                                         PercentAbove10EmaFalling = ii.PercentAbove10EmaFalling
                                     }).ToListAsync();

                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the index data", ex);
            }
            return indices;
        }

                                    //where si.DoubleTop == marketSummaryDTO.BullishPercentRising || marketSummaryDTO.BullishPercentRising == false
                                    //where si.RsRising == marketSummaryDTO.PercentRsRisingRising || marketSummaryDTO.PercentRsRisingRising == false
                                    //where si.ClosedAboveEma10 == marketSummaryDTO.PercentAbove10EmaRising || marketSummaryDTO.PercentAbove10EmaRising == false
                                    //where si.ClosedAboveEma30 == marketSummaryDTO.PercentAbove30EmaRising || marketSummaryDTO.PercentAbove30EmaRising == false
                                    //where si.AboveBullSupport == marketSummaryDTO.PercentPositiveTrendRising || marketSummaryDTO.PercentPositiveTrendRising == false

        public async Task<IEnumerable<ShareSummaryDTO>> GetShareValuesAsync(MarketSummaryDTO marketSummaryDTO)
        {
            IEnumerable<ShareSummaryDTO> shares = new List<ShareSummaryDTO>();
            try
            {
                using (var db = new PnFDataContext())
                {
                    shares = await (from si in db.ShareIndicators
                                    join s in db.Shares on si.ShareId equals s.Id
                                    join rs in db.ShareRSIValues on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Market } equals new { rs.ShareId, rs.Day, rs.RelativeTo }
                                    join prs in db.ShareRSIValues on new { si.ShareId, si.Day, RelativeTo = RelativeToEnum.Sector } equals new { prs.ShareId, prs.Day, prs.RelativeTo }
                                    join q in db.EodPrices on new { si.ShareId, si.Day } equals new { q.ShareId, q.Day }
                                    where si.Day == marketSummaryDTO.Day
                                    where s.ExchangeCode == marketSummaryDTO.ExchangeCode
                                    where s.ExchangeSubCode == marketSummaryDTO.ExchangeSubCode
                                    where s.SuperSector == marketSummaryDTO.SuperSector || marketSummaryDTO.SuperSector == null
                                    orderby s.Tidm
                                    select new ShareSummaryDTO()
                                    {
                                        Id = s.Id,
                                        Tidm = s.Tidm,
                                        Name = s.Name,
                                        Close = q.Close,
                                        RsValue = rs.Value,
                                        PeerRsValue = prs.Value,
                                        Ema10 = si.Ema10??0d,
                                        Ema30 = si.Ema30??0d,
                                        ClosedAboveEma10 = si.ClosedAboveEma10??false,
                                        ClosedAboveEma30 = si.ClosedAboveEma30??false,
                                        Rising = si.Rising??false,
                                        DoubleTop = si.DoubleTop??false,
                                        TripleTop = si.TripleTop??false,
                                        RsRising = si.RsRising??false,
                                        RsBuy = si.RsBuy??false,
                                        PeerRsRising = si.PeerRsRising??false,
                                        PeerRsBuy = si.PeerRsBuy??false,
                                        Falling = si.Falling??false,
                                        DoubleBottom = si.DoubleBottom??false,
                                        TripleBottom = si.TripleBottom??false,
                                        RsFalling = si.RsFalling??false,
                                        RsSell = si.RsSell??false,
                                        PeerRsFalling = si.PeerRsFalling??false,
                                        PeerRsSell = si.PeerRsSell??false,
                                        AboveBullSupport = si.AboveBullSupport
                                    }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(this, LogType.Error, "An error occurred loading the share values data", ex);
            }
            return shares;

        }
    }
}
