using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PnFData.Model;
using PnFData.Services;
using PnFDesktop.Classes;

namespace PnFDesktop.Services
{
    public static class ChartBuilderService
    {
        public static PnFChart GenerateHiLoChart(string tidm, int reversal, DateTime uptoDate)
        {
            List<Eod> tickData = null;
            PnFChart? newChart = null;
            // Retrieve the data
            try
            {
                MessageLog.LogMessage(null, LogType.Information, $@"Retrieving tick data for {tidm}.");
                using (PnFDataContext db = new PnFDataContext())
                {
                    tickData = db.Shares.Where(s => s.Tidm == tidm)
                        .Select(s => s.EodPrices).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageLog.LogMessage(null, LogType.Error, $@"Error retrieving tick data for {tidm}.");
                MessageLog.LogMessage(null, LogType.Error, ex.Message);
            }

            if (tickData != null && tickData.Any())
            {
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFHiLoChartBuilderService(tickData);
                double boxSize = chartBuilder.ComputeBoxSize();
                try
                {
                    MessageLog.LogMessage(null, LogType.Information, $@"Building chart for {tidm}.");
                    newChart = chartBuilder.BuildChart(boxSize, reversal, uptoDate);
                    if (newChart != null)
                    {
                        newChart.Source = PnFChartSource.Share;
                        newChart.Name = $"{tidm.Replace(".LON", "")} Daily (H/L) ({newChart.BoxSize}, {reversal} rev)";
                    }
                    MessageLog.LogMessage(null, LogType.Information, $@"Box size is {newChart.BoxSize}.");

                }
                catch (Exception ex)
                {
                    MessageLog.LogMessage(null, LogType.Error, $@"Error building chart for {tidm}.");
                    MessageLog.LogMessage(null, LogType.Error, ex.Message);
                }
            }
            else
            {
                MessageLog.LogMessage(null, LogType.Information, $@"Tick data not available.");
            }

            return newChart;
        }

    }
}
