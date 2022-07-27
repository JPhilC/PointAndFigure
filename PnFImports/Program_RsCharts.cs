using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PnFData.Interfaces;
using PnFData.Model;
using PnFData.Services;
using System.Data;

namespace PnFImports
{
    internal partial class PnFImports
    {
        internal static void GenerateShareRSCharts(string exchangeCode, DateTime toDate)
        {
            try
            {
                List<Share> shares = new();
                using (PnFDataContext db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        shares = db.Shares.Where(s => s.EodPrices.Any()).ToList();
                    }
                    else
                    {
                        shares = db.Shares.Where(s => s.ExchangeCode == exchangeCode && s.EodPrices.Any()).ToList();
                    }
                }
                _progress = 0;
                _total = shares.Count * 2.0;
                Parallel.ForEach(shares,
                    new ParallelOptions { MaxDegreeOfParallelism = 5 }, (s) =>
                    {
                        GenerateShareRSChartPair(s.Id, s.Tidm, toDate);
                    });
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Generate share RS charts failed.");
                Console.WriteLine(ex.Message);
            }
        }

        internal static void GenerateShareRSChart(string tidm, string relativeTo, DateTime uptoDate)
        {
            List<ShareRSI> tickData;
            try
            {
                Console.WriteLine($@"Retrieving RS tick data for {tidm}.");
                using (PnFDataContext db = new PnFDataContext())
                {
                    tickData = db.Shares.Where(s => s.Tidm == tidm)
                        .Select(s => s.RSIValues).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error retrieving tick data for {tidm}.");
                Console.WriteLine(ex.Message);
                return;
            }
            if (tickData != null)
            {
                List<IDayValue> chartData;
                Guid shareId = tickData.FirstOrDefault()!.ShareId;
                if (relativeTo == "MARKET")
                {
                    chartData = tickData.Where(d => d.RelativeTo == RelativeToEnum.Market).ToList<IDayValue>();
                    GenerateRSChart(shareId, tidm, uptoDate, chartData, PnFChartSource.RSStockVMarket);
                }
                else if (relativeTo == "SECTOR")
                {

                    chartData = tickData.Where(d => d.RelativeTo == RelativeToEnum.Sector).ToList<IDayValue>();
                    GenerateRSChart(shareId, tidm, uptoDate, chartData, PnFChartSource.RSStockVSector);
                }
            }

        }

        internal static void GenerateShareRSChartPair(Guid shareId, string tidm, DateTime uptoDate)
        {
            List<ShareRSI> tickData;
            // Retrieve the data
            try
            {
                Console.WriteLine($@"Retrieving RS tick data for {tidm}.");
                using (PnFDataContext db = new PnFDataContext())
                {
                    tickData = db.Shares.Where(s => s.Tidm == tidm)
                        .Select(s => s.RSIValues).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error retrieving tick data for {tidm}.");
                Console.WriteLine(ex.Message);
                return;
            }

            if (tickData != null)
            {
                List<IDayValue> chartData = tickData.Where(d => d.RelativeTo == RelativeToEnum.Market).ToList<IDayValue>();
                GenerateRSChart(shareId, tidm, uptoDate, chartData, PnFChartSource.RSStockVMarket);
                UpdateProgress();

                chartData = tickData.Where(d => d.RelativeTo == RelativeToEnum.Sector).ToList<IDayValue>();
                GenerateRSChart(shareId, tidm, uptoDate, chartData, PnFChartSource.RSStockVSector);
                UpdateProgress();
            }
        }
        internal static void GenerateIndexRSCharts(string exchangeCode, DateTime toDate)
        {
            try
            {
                List<PnFData.Model.Index> indices;
                using (PnFDataContext db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        indices = db.Indices.Where(s => s.SuperSector != null && s.IndexValues.Any()).ToList();
                    }
                    else
                    {
                        indices = db.Indices.Where(s => s.ExchangeCode == exchangeCode && s.SuperSector != null && s.IndexValues.Any()).ToList();
                    }
                }
                _progress = 0;
                _total = indices.Count;
                Parallel.ForEach(indices,
                    new ParallelOptions { MaxDegreeOfParallelism = 10 }, (i) =>
                    {
                        GenerateIndexRSChart(i.Id, $"{i.ExchangeCode}, {i.ExchangeSubCode}, {i.SuperSector}", toDate);
                        UpdateProgress();
                    });
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Generate Index RS charts failed.");
                Console.WriteLine(ex.Message);
            }

        }

        internal static void GenerateIndexRSChart(Guid indexId, string indexName, DateTime uptoDate)
        {
            List<IDayValue> tickData = null; ;
            // Retrieve the data
            try
            {
                Console.WriteLine($@"Retrieving RS tick data for {indexName}.");
                using (PnFDataContext db = new PnFDataContext())
                {
                    var rawTickData = db.Indices.Where(i => i.Id == indexId)
                        .Select(s => s.RSIValues).FirstOrDefault();
                    if (rawTickData != null)
                    {
                        tickData = rawTickData.ToList<IDayValue>();
                    }
                }
                if (tickData != null)
                {
                    GenerateRSChart(indexId, indexName, uptoDate, tickData, PnFChartSource.RSSectorVMarket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error retrieving tick data for {indexName}.");
                Console.WriteLine(ex.Message);
                return;
            }
        }



        internal static void GenerateRSChart(Guid itemId, string tidm, DateTime uptoDate, List<IDayValue> tickData, PnFChartSource chartSource)
        {
            PnFChart? newChart = null;
            int reversal = 3;
            if (tickData != null && tickData.Any())
            {
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFLogarithmicValueChartBuilderService(tickData);
                double boxSize = 2d;
                PnFChart chart = null;
                try
                {
                    using (var db = new PnFDataContext())
                    {

                        // See if we have an existing chart
                        if (chartSource == PnFChartSource.RSSectorVMarket)
                        {
                            // Looking for an index chart
                            // Get the index
                            var index = db.Indices
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == itemId);


                            // Try retrieving the existing chart
                            if (index != null)
                            {
                                if (index.Charts.Any())
                                {
                                    var chartId = index.Charts.Where(c =>
                                                c.Chart != null
                                                && c.Chart.BoxSize == boxSize
                                                && c.Chart.Reversal == reversal
                                                && c.Chart.Source == chartSource)
                                        .OrderByDescending(c => c.Chart.GeneratedDate)
                                        .Select(c => c.Chart.Id)
                                        .FirstOrDefault();
                                    chart = db.PnFCharts
                                        .Include(c => c.Columns.OrderBy(c => c.Index)).ThenInclude(l => l.Boxes.OrderBy(b => b.Index))
                                        .SingleOrDefault(c => c.Id == chartId);
                                }
                            }
                        }
                        else
                        {
                            // Looking for a share chart
                            // Get the share
                            var share = db.Shares
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == itemId);


                            // Try retrieving the existing chart
                            if (share != null)
                            {
                                if (share.Charts.Any())
                                {
                                    var chartId = share.Charts.Where(c =>
                                                c.Chart != null
                                                && c.Chart.BoxSize == boxSize
                                                && c.Chart.Reversal == reversal
                                                && c.Chart.Source == chartSource)
                                        .OrderByDescending(c => c.Chart.GeneratedDate)
                                        .Select(c => c.Chart.Id)
                                        .FirstOrDefault();
                                    chart = db.PnFCharts
                                        .Include(c => c.Columns.OrderBy(c => c.Index)).ThenInclude(l => l.Boxes.OrderBy(b => b.Index))
                                        .SingleOrDefault(c => c.Id == chartId);
                                }
                            }

                        }
                        if (chart != null)
                        {
                            if (chart.Columns.Count > 0)
                            {
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
                            else
                            {
                                // Delete chart with no columns and treat as a new chart
                                Console.WriteLine("Note: Removing chart with zero columns");
                                db.PnFCharts.Remove(chart);
                                bool saved = false;
                                int trys = 0;
                                while (!saved && trys < 5)
                                {
                                    try
                                    {
                                        int saveResult = db.SaveChanges();
                                        saved = true;
                                        chart = null;
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

                if (chart == null)
                {
                    // Need to buid a new chart.
                    try
                    {
                        Console.WriteLine($@"Building {chartSource} chart for {tidm}.");
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
                    newChart.Source = chartSource;
                    if (chartSource == PnFChartSource.RSSectorVMarket)
                    {
                        // Sector RS charts
                        newChart.Name = $"{tidm} Sector RS ({newChart.BoxSize}%, {reversal} rev)";
                        try
                        {
                            // Save the chart
                            using (var db = new PnFDataContext())
                            {
                                // Get the index
                                var ndx = db.Indices
                                    .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                    .SingleOrDefault(s => s.Id == itemId);


                                // Try retrieving the existing chart
                                if (ndx != null)
                                {
                                    if (ndx.Charts.Any())
                                    {
                                        var indexCharts = ndx.Charts.Where(c =>
                                            c.Chart != null
                                            && c.Chart.BoxSize == newChart.BoxSize
                                            && c.Chart.Reversal == newChart.Reversal
                                            && c.Chart.Source == chartSource
                                            && c.Chart.GeneratedDate == newChart.GeneratedDate);
                                        foreach (var indexChart in indexCharts.ToList())
                                        {
                                            db.Remove(indexChart.Chart);
                                            ndx.Charts.Remove(indexChart);
                                        }
                                    }

                                    IndexChart newIndexChart = new IndexChart()
                                    {
                                        Chart = newChart,

                                    };
                                    ndx.Charts.Add(newIndexChart);
                                    db.Update(ndx);
                                    bool saved = false;
                                    int trys = 0;
                                    while (!saved && trys < 5)
                                    {
                                        try
                                        {
                                            int saveResult = db.SaveChanges();
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
                    else
                    {
                        // Stock RS charts
                        if (chartSource == PnFChartSource.RSStockVMarket)
                        {
                            newChart.Name = $"{tidm.Replace(".LON", "")} Market RS ({newChart.BoxSize}%, {reversal} rev)";
                        }
                        else if (chartSource == PnFChartSource.RSStockVSector)
                        {
                            newChart.Name = $"{tidm.Replace(".LON", "")} Peer RS ({newChart.BoxSize}%, {reversal} rev)";
                        }
                        try
                        {
                            // Save the chart
                            using (var db = new PnFDataContext())
                            {
                                // Get the share
                                var share = db.Shares
                                    .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                    .SingleOrDefault(s => s.Id == itemId);


                                // Try retrieving the existing chart
                                if (share != null)
                                {
                                    if (share.Charts.Any())
                                    {
                                        var shareCharts = share.Charts.Where(c =>
                                            c.Chart != null
                                            && c.Chart.BoxSize == newChart.BoxSize
                                            && c.Chart.Reversal == newChart.Reversal
                                            && c.Chart.Source == chartSource
                                            && c.Chart.GeneratedDate == newChart.GeneratedDate);
                                        foreach (var shareChart in shareCharts.ToList())
                                        {
                                            db.Remove(shareChart.Chart);
                                            share.Charts.Remove(shareChart);
                                        }
                                    }

                                    ShareChart newShareChart = new ShareChart()
                                    {
                                        Chart = newChart,

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
            }
            else
            {
                Console.WriteLine($@"Tick data not available.");
                return;
            }


        }

    }
}
