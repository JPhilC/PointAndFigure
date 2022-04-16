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
    }
}
