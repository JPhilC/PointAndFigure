using Microsoft.EntityFrameworkCore;
using PnFData.Interfaces;
using PnFData.Model;
using PnFData.Services;
using System.Data;

namespace PnFImports
{
    internal partial class PnFImports
    {
        internal static void GenerateIndexCharts(string exchangeCode)
        {
            List<PnFData.Model.Index> indices = null;
            try
            {
                using (PnFDataContext db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        indices = db.Indices.Where(s => s.IndexValues.Any()).ToList();
                    }
                    else
                    {
                        indices = db.Indices.Where(s => s.ExchangeCode == exchangeCode && s.IndexValues.Any()).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Generate Index charts failed retrieving indexes.");
                Console.WriteLine(ex.Message);
                return;
            }
            if (indices != null)
            {
                DateTime now = DateTime.Now.Date;
                string indexName = "";
                Parallel.ForEach(indices,
                    new ParallelOptions { MaxDegreeOfParallelism = 5 },
                (index) =>
                {
                    if (string.IsNullOrEmpty(index.SuperSector))
                    {
                        indexName = $"{index.ExchangeCode}, {index.ExchangeSubCode}";
                    }
                    else
                    {
                        indexName = $"{index.ExchangeCode}, {index.ExchangeSubCode}, {index.SuperSector}";
                    }

                    List<IDayValue> tickData = null; ;
                    // Retrieve the data
                    try
                    {
                        Console.WriteLine($@"Retrieving index value tick data for {indexName}.");
                        using (PnFDataContext db = new PnFDataContext())
                        {
                            var rawTickData = db.IndexValues.Where(i => i.IndexId == index.Id);

                            if (rawTickData != null)
                            {
                                tickData = rawTickData.Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.Value
                                }
                                ).ToList<IDayValue>();

                                GenerateIndexChart(index.Id, indexName, now, tickData, PnFChartSource.Index);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Error retrieving tick data for {indexName}.");
                        Console.WriteLine(ex.Message);
                    }

                });
            }

        }


        internal static void GenerateIndexChart(Guid indexId, string indexName, DateTime uptoDate, List<IDayValue> tickData, PnFChartSource chartSource)
        {
            PnFChart? newChart = null;
            int reversal = 3;
            if (tickData != null && tickData.Any())
            {
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFSingleValueChartBuilderService(tickData);
                double boxSize = chartBuilder.ComputeBoxSize();
                PnFChart chart = null;
                try
                {
                    using (var db = new PnFDataContext())
                    {
                        // Get the index
                        var ndx = db.Indices
                            .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                            .SingleOrDefault(s => s.Id == indexId);


                        // Try retrieving the existing chart
                        if (ndx != null)
                        {
                            if (ndx.Charts.Any())
                            {
                                var chartId = ndx.Charts.Where(c =>
                                    c.Chart != null
                                    && c.Chart.BoxSize == boxSize
                                    && c.Chart.Reversal == reversal
                                    && c.Chart.Source == chartSource)
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
                                while (!saved)
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
                    try
                    {
                        Console.WriteLine($@"Building {chartSource} chart for {indexName}.");
                        newChart = chartBuilder.BuildChart(boxSize, reversal, uptoDate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Error building {chartSource} chart for {indexName}.");
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }

                if (newChart != null)
                {
                    newChart.Source = chartSource;
                    // Sector RS charts
                    newChart.Name = $"{indexName} {chartSource} ({newChart.BoxSize}, {reversal} rev)";
                    try
                    {
                        // Save the chart
                        using (var db = new PnFDataContext())
                        {
                            // Get the index
                            var ndx = db.Indices
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == indexId);


                            // Try retrieving the existing chart
                            if (ndx != null)
                            {
                                if (ndx.Charts.Any())
                                {
                                    var indexCharts = ndx.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.BoxSize == newChart.BoxSize
                                        && c.Chart.Reversal == newChart.Reversal
                                        && c.Chart.Source == chartSource);
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
                                while (!saved)
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


        internal static void GenerateIndexPercentCharts(string exchangeCode)
        {
            List<PnFData.Model.Index> indices = null;
            try
            {
                using (PnFDataContext db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        indices = db.Indices.Where(s => s.IndexValues.Any()).ToList();
                    }
                    else
                    {
                        indices = db.Indices.Where(s => s.ExchangeCode == exchangeCode && s.IndexValues.Any()).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Generate Index percent charts failed retrieving indexes.");
                Console.WriteLine(ex.Message);
                return;
            }
            if (indices != null)
            {
                DateTime now = DateTime.Now.Date;
                string indexName = "";
                foreach (var index in indices)
                {
                    if (string.IsNullOrEmpty(index.SuperSector))
                    {
                        indexName = $"{index.ExchangeCode}, {index.ExchangeSubCode}";
                    }
                    else
                    {
                        indexName = $"{index.ExchangeCode}, {index.ExchangeSubCode}, {index.SuperSector}";
                    }

                    List<IDayValue> tickData = null; ;
                    // Retrieve the data
                    try
                    {
                        Console.WriteLine($@"Retrieving RS tick data for {indexName}.");
                        using (PnFDataContext db = new PnFDataContext())
                        {
                            var rawTickData = db.IndexValues.Where(i => i.IndexId == index.Id);

                            if (rawTickData != null)
                            {
                                tickData = rawTickData.Where(r => r.BullishPercent.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.BullishPercent.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexBullishPercent);

                                tickData = rawTickData.Where(r => r.PercentAboveEma10.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.PercentAboveEma10.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexPercentShareAbove10);

                                tickData = rawTickData.Where(r => r.PercentAboveEma30.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.PercentAboveEma30.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexPercentShareAbove30);

                                tickData = rawTickData.Where(r => r.PercentPositiveTrend.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.PercentPositiveTrend.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexPercentSharePT);

                                tickData = rawTickData.Where(r => r.PercentRsBuy.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.PercentRsBuy.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexPercentShareRsBuy);

                                tickData = rawTickData.Where(r => r.PercentRsRising.HasValue).Select(r => new SimpleDayValue()
                                {
                                    Day = r.Day,
                                    Value = r.PercentRsRising.Value
                                }
                                ).ToList<IDayValue>();

                                GeneratePercentChart(index.Id, indexName, now, tickData, PnFChartSource.IndexPercentShareRsX);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Error retrieving tick data for {indexName}.");
                        Console.WriteLine(ex.Message);
                    }

                };
            }

        }


        internal static void GeneratePercentChart(Guid indexId, string indexName, DateTime uptoDate, List<IDayValue> tickData, PnFChartSource chartSource)
        {
            PnFChart? newChart = null;
            int reversal = 3;
            if (tickData != null && tickData.Any())
            {
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFSingleValueChartBuilderService(tickData);
                double boxSize = 2.0d;
                PnFChart chart = null;
                try
                {
                    using (var db = new PnFDataContext())
                    {
                        // Get the index
                        var ndx = db.Indices
                            .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                            .SingleOrDefault(s => s.Id == indexId);


                        // Try retrieving the existing chart
                        if (ndx != null)
                        {
                            if (ndx.Charts.Any())
                            {
                                var chartId = ndx.Charts.Where(c =>
                                    c.Chart != null
                                    && c.Chart.BoxSize == boxSize
                                    && c.Chart.Reversal == reversal
                                    && c.Chart.Source == chartSource)
                                    .OrderByDescending(c => c.Chart.GeneratedDate)
                                    .Select(c => c.Chart.Id)
                                    .FirstOrDefault();
                                chart = db.PnFCharts
                                    .Include(c => c.Columns).ThenInclude(l => l.Boxes)
                                    .SingleOrDefault(c => c.Id == chartId);
                            }

                            if (chart != null)
                            {
                                // Update the existing chart
                                if (chartBuilder.UpdateChart(ref chart, uptoDate))
                                {
                                    db.Update(ndx);

                                    bool saved = false;
                                    while (!saved)
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
                    try
                    {
                        Console.WriteLine($@"Building {chartSource} chart for {indexName}.");
                        newChart = chartBuilder.BuildChart(boxSize, reversal, uptoDate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($@"Error building {chartSource} chart for {indexName}.");
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }

                if (newChart != null)
                {
                    newChart.Source = chartSource;
                    // Sector RS charts
                    newChart.Name = $"{indexName} {chartSource} ({newChart.BoxSize}, {reversal} rev)";
                    try
                    {
                        // Save the chart
                        using (var db = new PnFDataContext())
                        {
                            // Get the index
                            var ndx = db.Indices
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == indexId);


                            // Try retrieving the existing chart
                            if (ndx != null)
                            {
                                if (ndx.Charts.Any())
                                {
                                    var indexCharts = ndx.Charts.Where(c =>
                                        c.Chart != null
                                        && c.Chart.BoxSize == newChart.BoxSize
                                        && c.Chart.Reversal == newChart.Reversal
                                        && c.Chart.Source == chartSource);
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
                                while (!saved)
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
