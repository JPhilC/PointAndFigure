using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PnFData.Model;
using PnFData.Services;
using System.Data;

namespace PnFImports
{
    internal partial class PnFImports
    {
        internal static void GenerateAllHiLoCharts(string exchangeCode, DateTime toDate)
        {
            try
            {
                List<string> tidms = new();
                using (PnFDataContext db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        tidms = db.Shares.Where(s => s.EodPrices.Any()).Select(s => s.Tidm).ToList();
                    }
                    else
                    {
                        tidms = db.Shares.Where(s => s.ExchangeCode == exchangeCode && s.EodPrices.Any()).Select(s => s.Tidm).ToList();
                    }
                }
                Parallel.ForEach(tidms,
                    new ParallelOptions { MaxDegreeOfParallelism = 10 }, (tidm) => GenerateHiLoChart(tidm, toDate));
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! generate HiLo charts failed.");
                Console.WriteLine(ex.Message);
            }
        }

        internal static void GenerateHiLoChart(string tidm, DateTime uptoDate)
        {
            List<Eod> tickData;
            PnFChart? newChart = null;
            int reversal = 3;
            // Retrieve the data
            try
            {
                Console.WriteLine($@"Retrieving tick data for {tidm}.");
                using (PnFDataContext db = new PnFDataContext())
                {
                    tickData = db.Shares.Where(s => s.Tidm == tidm)
                        .Select(s => s.EodPrices).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error retrieving tick data for {tidm}.");
                Console.WriteLine(ex.Message);
                return;
            }

            if (tickData != null && tickData.Any())
            {

                PnFChartBuilderService chartBuilder = new PnFHiLoChartBuilderService(tickData);
                double boxSize = chartBuilder.ComputeBoxSize();
                PnFChart chart = null;
                // See if we already have a chart.
                try
                {
                    using (var db = new PnFDataContext())
                    {
                        // Get the share
                        var share = db.Shares
                            .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                            .SingleOrDefault(s => s.Tidm == tidm);


                        // Try retrieving the existing chart
                        if (share != null)
                        {
                            if (share.Charts.Any())
                            {
                                var chartId = share.Charts.Where(c =>
                                            c.Chart != null
                                            && c.Chart.BoxSize == boxSize
                                            && c.Chart.Reversal == reversal
                                            && c.Chart.Source == PnFChartSource.Share)
                                    .OrderByDescending(c => c.Chart.GeneratedDate)
                                    .Select(c => c.Chart.Id)
                                    .FirstOrDefault();
                                chart = db.PnFCharts
                                    .Include(c => c.Columns).ThenInclude(l => l.Boxes)
                                    .SingleOrDefault(c => c.Id == chartId);
                            }
                        }

                        if (chart != null)
                        {
                            // Update the existing chart
                            if (chartBuilder.UpdateChart(ref chart, uptoDate))
                            {
                                // Try to save any changes
                                db.Update(chart);

                                bool saved = false;
                                int trys = 0;
                                while (!saved && trys < 5)
                                {
                                    try
                                    {
                                        int saveResult = db.SaveChanges();
                                        Console.WriteLine($"{saveResult} record saved.");
                                        saved = true;
                                    }
                                    catch (DbUpdateConcurrencyException updateEx)
                                    {
                                        foreach (var entry in updateEx.Entries)
                                        {
                                            var proposedValues = entry.CurrentValues;
                                            var databaseValues = entry.GetDatabaseValues();
                                            proposedValues["Version"] = databaseValues["Version"];
                                            entry.OriginalValues.SetValues(proposedValues);
                                        }
                                    }
                                    catch (SqlException sqex)
                                    {
                                        trys++;
                                        System.Diagnostics.Debug.WriteLine($"SQLException {sqex.GetType()}, message {sqex.Message}");
                                        Thread.Sleep(PnFChartBuilderService.GetRandomDelay()); // Wait a randomn delay between 1 and 5 seconds
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem update/saving the chart.");
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                }



                // Create a new chart.
                if (chart == null)
                {
                    try
                    {
                        Console.WriteLine($@"Building chart for {tidm}.");
                        newChart = chartBuilder.BuildChart(boxSize, reversal, uptoDate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Error building chart for {tidm}.");
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }

                if (newChart != null)
                {
                    newChart.Source = PnFChartSource.Share;
                    newChart.Name = $"{tidm.Replace(".LON", "")} Daily (H/L) ({newChart.BoxSize}, {reversal} rev)";
                    try
                    {
                        // Save the chart
                        using (var db = new PnFDataContext())
                        {
                            // Get the share
                            var share = db.Shares
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Tidm == tidm);


                            // Try retrieving the existing chart
                            if (share != null)
                            {
                                if (share.Charts.Any())
                                {
                                    var shareCharts = share.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.BoxSize == newChart.BoxSize
                                        && c.Chart.Reversal == newChart.Reversal
                                        && c.Chart.Source == PnFChartSource.Share
                                        && c.Chart.GeneratedDate == newChart.GeneratedDate);
                                    foreach (var shareChart in shareCharts.ToList())
                                    {
                                        db.Remove(shareChart.Chart);
                                        share.Charts.Remove(shareChart);
                                    }
                                }

                                ShareChart newShareChart = new ShareChart()
                                {
                                    Chart = newChart
                                };
                                share.Charts.Add(newShareChart);
                                db.Update(share);

                                bool saved = false;
                                int trys = 0;
                                while (!saved && trys < 5)
                                {
                                    try
                                    {
                                        int saveResult = db.SaveChanges();
                                        Console.WriteLine($"{saveResult} record saved.");
                                        saved = true;
                                    }
                                    catch (DbUpdateConcurrencyException updateEx)
                                    {
                                        foreach (var entry in updateEx.Entries)
                                        {
                                            var proposedValues = entry.CurrentValues;
                                            var databaseValues = entry.GetDatabaseValues();
                                            proposedValues["Version"] = databaseValues["Version"];
                                            entry.OriginalValues.SetValues(proposedValues);
                                        }
                                    }
                                    catch (SqlException sqex)
                                    {
                                        trys++;
                                        System.Diagnostics.Debug.WriteLine($"SQLException {sqex.GetType()}, message {sqex.Message}");
                                        Thread.Sleep(PnFChartBuilderService.GetRandomDelay()); // Wait a randomn delay between 1 and 5 seconds
                                    }
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("There was a problem saving the chart.");
                        Console.WriteLine(ex.Message);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine(ex.InnerException.Message);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($@"Tick data not available.");
                return;
            }

        }


    }
}
