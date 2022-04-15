using Microsoft.EntityFrameworkCore;
using PnFData.Model;
using PnFDesktop.Classes;
using PnFDesktop.Interfaces;
using System;
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
            try {
            using (var db = new PnFDataContext())
            {
                if (chartSource == PnFChartSource.RSSectorVMarket)
                {
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
                }
                else
                {
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
                }
            }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(null, LogType.Error, "An error occurred loading the PnF chart data", ex);
            }
            return chart;
        }
    }
}
