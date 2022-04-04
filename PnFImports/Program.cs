using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CsvHelper;
using CsvHelper.Configuration;
using PnFData.Model;
using PnFImports.Model;
using PnFImports.Services;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using PnFData.Services;
using PnFData.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PnFImports
{
    internal class PnFImports
    {
        private static int _LastReturnValue = -1;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "shares":
                        PnFImports.ImportShares();
                        break;

                    case "history":
                        PnFImports.ImportEodHistoricPrices();
                        break;

                    case "daily":
                        if (args.Length > 1)
                        {
                            PnFImports.ImportEodDailyPrices(args[1].ToUpper());
                        }
                        else
                        {
                            PnFImports.ImportEodDailyPrices(null);
                        }
                        break;

                    case "hilocharts":
                        GenerateAllHiLoCharts();
                        break;

                    case "sharerscharts":
                        GenerateShareRSCharts();
                        break;

                    case "indexrscharts":
                        GenerateIndexRSCharts();
                        break;

                    case "indexpercentcharts":
                        GenerateIndexPercentCharts();
                        break;

                    case "fullrun":
                        FullRun();
                        break;

                    case "hilochart":
                        if (args.Length > 2)
                        {
                            PnFImports.GenerateHiLoChart(args[1].ToUpper(),
                                DateTime.ParseExact(args[2], "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None));
                        }
                        break;

                }
            }

            Console.WriteLine("Completed. Press a key to exit.");
            Console.ReadKey();

        }

        internal static void FullRun()
        {
            Console.WriteLine("Starting full run ...");
            _LastReturnValue = 0;
            ImportEodDailyPrices(null);

            if (_LastReturnValue == 0)
            {
                // Generate values;
                RunLongStoredProcedure("uspGenerateDailyValues", 60);
            }


            if (_LastReturnValue == 0)
            {
                // Generate charts (HiLo and ShareRS may have concurrency issues so process separately)
                Task hiLoCharts = Task.Run(() => GenerateAllHiLoCharts());
                Task indexRsCharts = Task.Run(() => GenerateIndexRSCharts());
                Task.WaitAll(new Task[] { hiLoCharts, indexRsCharts });
                Task.WaitAll(Task.Run(() => GenerateShareRSCharts()));

            }

            if (_LastReturnValue == 0)
            {
                // Generate SIB Indicators
                RunLongStoredProcedure("uspUpdateSIBIndicators", 60);
            }

            if (_LastReturnValue == 0)
            {
                // Generate index percent Charts
                GenerateIndexPercentCharts();
            }

            if (_LastReturnValue == 0)
            {
                // Generate the bullish percent and other percentage stats
                RunLongStoredProcedure("uspUpdateIndexPercentIndicators", 30);
            }

            if (_LastReturnValue == 0)
            {
                Console.WriteLine("Full run completed OK.");
            }
            else
            {
                Console.WriteLine("Error! Full run failed.");
            }
        }

        internal static void ImportShares()
        {
            Console.WriteLine("Importing Shares");
            using var db = new PnFDataContext();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(@"C:\Users\phil\Downloads\Filtered_LSE shares_CompanyExport.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ShareScopeCompany>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (ShareScopeCompany data in records)
            {
                string tidm = data.Tidm + ".LON";
                string shareScopeId = data.ShareScopeID.ToString();
                var share = db.Shares.FirstOrDefault(b =>
                    b.ShareDataSource == "SS" & b.ShareDataSourceId == shareScopeId);
                if (share == null)
                {
                    Console.Write($"Inserting {tidm}: {data.Name} ...");
                    db.Add(new Share()
                    {
                        Tidm = tidm,
                        Name = data.Name,
                        ExchangeCode = data.ExchangeCode,
                        ExchangeSubCode = data.ExchangeSubCode,
                        SharesInIssueMillions = data.SharesInIssue,
                        MarketCapMillions = data.MarketCap,
                        SuperSector = data.Supersector,
                        Sector = data.Sector,
                        PricesCurrency = data.Currency,
                        ShareDataSource = "SS", // Share Scope
                        ShareDataSourceId = data.ShareScopeID.ToString() // Share Scope ID

                    }
                    );
                }
                else
                {
                    Console.Write($"Updating {tidm}: {data.Name} ...");
                    share.Tidm = tidm;
                    share.Name = data.Name;
                    share.ExchangeCode = data.ExchangeCode;
                    share.ExchangeSubCode = data.ExchangeSubCode;
                    share.SharesInIssueMillions = data.SharesInIssue;
                    share.MarketCapMillions = data.MarketCap;
                    share.SuperSector = data.Supersector;
                    share.Sector = data.Sector;
                    share.PricesCurrency = data.Currency;
                    db.Update(share);
                }

                try
                {
                    db.SaveChanges();
                    Console.WriteLine(" OK.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error!");
                    Console.WriteLine(ex.Message);
                    _LastReturnValue = 1;   // Signal an error.
                }
            }
        }

        internal static void ImportEodHistoricPrices()
        {
            DateTime cutOffDate = new(2016, 12, 31);
            List<ShareSummary> shareIds = new List<ShareSummary>();
            using (var db = new PnFDataContext())
            {
                shareIds = db.Shares.Select(s => new ShareSummary()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Tidm = s.Tidm,
                    EodErrors = s.EodError,
                    LastEodDate = s.LastEodDate,
                    HasPrices = s.EodPrices.Any()
                }).OrderBy(s => s.Tidm).ToList();
            }

            foreach (var shareData in shareIds)
            {
                if (shareData.EodErrors)
                {
                    continue; // Skip as were no eod prices last time
                }

                // Catch up code to update LastEodDate
                if (shareData.HasPrices)
                {
                    if (shareData.LastEodDate == null)
                    {
                        using (var db = new PnFDataContext())
                        {
                            Console.WriteLine($"Updating {shareData.Tidm}: {shareData.Name}");
                            Share share = db.Shares.First(s => s.Id == shareData.Id);
                            share.LastEodDate = db.EodPrices.Where(p => p.ShareId == share.Id).Max(p => p.Day);
                            db.Update(share);
                            db.SaveChanges();
                        }
                    }

                    continue;
                }

                // Get the prices
                Task.Run((async () =>
                {
                    Console.Write($"Processing {shareData.Tidm}: {shareData.Name} ...");
                    using (var db = new PnFDataContext())
                    {
                        var prices =
                            await AlphaVantageService.GetTimeSeriesDailyPrices(shareData.Tidm, cutOffDate, true);
                        Share share = db.Shares.First(s => s.Id == shareData.Id);
                        var dayPrices = prices as Eod[] ?? prices.ToArray();
                        if (dayPrices.Any())
                        {
                            foreach (Eod dayPrice in dayPrices)
                            {
                                dayPrice.ShareId = shareData.Id;
                                db.EodPrices.Add(dayPrice);
                            }

                            DateTime maxDay = dayPrices.Max(p => p.Day);
                            share.LastEodDate = maxDay;
                            db.Update(share);

                            await db.SaveChangesAsync();
                            Console.WriteLine(" OK.");
                        }
                        else
                        {
                            share.EodError = true;
                            db.Update(share);
                            await db.SaveChangesAsync();
                            Console.WriteLine(" Error!");
                        }
                    }

                    Thread.Sleep(12000);
                })).Wait();
            }

        }

        internal static void ImportEodDailyPrices(string tidm)
        {
            try
            {
                DateTime cutOffDate = new(2016, 12, 31);
                DateTime lastClose = PreviousWorkDay(DateTime.Now.Date);
                List<ShareSummary> shareIds = new List<ShareSummary>();
                using (var db = new PnFDataContext())
                {
                    shareIds = db.Shares.Select(s => new ShareSummary()
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Tidm = s.Tidm,
                        EodErrors = s.EodError,
                        LastEodDate = s.LastEodDate,
                        HasPrices = s.EodPrices.Any()
                    }).OrderBy(s => s.Tidm).ToList();
                }

                foreach (var shareData in shareIds)
                {
                    if (tidm != null && shareData.Tidm != tidm)
                    {
                        continue;
                    }
                    Console.Write($"Processing {shareData.Tidm}: {shareData.Name} ...");
                    //if (shareData.EodErrors)
                    //{
                    //    Console.WriteLine("Skipped, previously in error!");
                    //    continue; // Skip as were no eod prices last time
                    //}

                    // Check if the current record is up to date.
                    if (shareData.LastEodDate >= lastClose)
                    {
                        Console.WriteLine("Skipped, up to date.");
                        continue;
                    }

                    // Get the prices
                    Task.Run((async () =>
                    {
                        using (var db = new PnFDataContext())
                        {
                            var prices = await AlphaVantageService.GetTimeSeriesDailyPrices(shareData.Tidm,
                                (shareData.LastEodDate ?? cutOffDate));
                            Share share = db.Shares.First(s => s.Id == shareData.Id);
                            var dayPrices = prices as Eod[] ?? prices.ToArray();
                            if (dayPrices.Any())
                            {
                                foreach (Eod dayPrice in dayPrices)
                                {
                                    dayPrice.ShareId = shareData.Id;
                                    db.EodPrices.Add(dayPrice);
                                }

                                DateTime maxDay = dayPrices.Max(p => p.Day);
                                share.LastEodDate = maxDay;
                                db.Update(share);

                                await db.SaveChangesAsync();
                                Console.WriteLine(" OK.");
                            }
                            else
                            {
                                share.EodError = true;
                                db.Update(share);
                                await db.SaveChangesAsync();
                                Console.WriteLine(" Error!");
                            }
                        }

                        Thread.Sleep(12000);
                    })).Wait();
                }
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Import Daily EOD prices failed.");
                Console.WriteLine(ex.Message);
            }

        }

        internal static void GenerateAllHiLoCharts()
        {
            try
            {
                List<string> tidms = new();
                using (PnFDataContext db = new PnFDataContext())
                {
                    tidms = db.Shares.Where(s => s.EodPrices.Any()).Select(s => s.Tidm).ToList();
                }
                DateTime now = DateTime.Now.Date;
                foreach (var tidm in tidms)
                {
                    GenerateHiLoChart(tidm, now);
                };
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
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFHiLoChartBuilderService(tickData);
                double boxSize = chartBuilder.ComputeBoxSize();
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
            else
            {
                Console.WriteLine($@"Tick data not available.");
                return;
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


                            int saveResult = db.SaveChanges();
                            Console.WriteLine($"{saveResult} record saved.");
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

        internal static void GenerateShareRSCharts()
        {
            try
            {
                List<Share> shares = new();
                using (PnFDataContext db = new PnFDataContext())
                {
                    shares = db.Shares.Where(s => s.EodPrices.Any()).ToList();
                }
                DateTime now = DateTime.Now.Date;
                foreach (var s in shares)
                {
                    GenerateShareRSChartPair(s.Id, s.Tidm, now);
                };
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Generate share RS charts failed.");
                Console.WriteLine(ex.Message);
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

                chartData = tickData.Where(d => d.RelativeTo == RelativeToEnum.Sector).ToList<IDayValue>();
                GenerateRSChart(shareId, tidm, uptoDate, chartData, PnFChartSource.RSStockVSector);
            }
        }

        internal static void GenerateIndexRSCharts()
        {
            try
            {
                List<PnFData.Model.Index> indices;
                using (PnFDataContext db = new PnFDataContext())
                {
                    indices = db.Indices.Where(s => s.SuperSector != null && s.IndexValues.Any()).ToList();
                }
                DateTime now = DateTime.Now.Date;
                foreach (var i in indices)
                {
                    GenerateIndexRSChart(i.Id, $"{i.ExchangeCode}, {i.ExchangeSubCode}, {i.SuperSector}", now);
                };
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



        internal static void GenerateRSChart(Guid shareId, string tidm, DateTime uptoDate, List<IDayValue> tickData, PnFChartSource chartSource)
        {
            PnFChart? newChart = null;
            int reversal = 3;
            if (tickData != null && tickData.Any())
            {
                // Create the chart.
                PnFChartBuilderService chartBuilder = new PnFSingleValueChartBuilderService(tickData);
                double boxSize = chartBuilder.ComputeBoxSize();
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
            else
            {
                Console.WriteLine($@"Tick data not available.");
                return;
            }

            if (newChart != null)
            {
                newChart.Source = chartSource;
                if (chartSource == PnFChartSource.RSSectorVMarket)
                {
                    // Sector RS charts
                    newChart.Name = $"{tidm} Sector RS ({newChart.BoxSize}, {reversal} rev)";
                    try
                    {
                        // Save the chart
                        using (var db = new PnFDataContext())
                        {
                            // Get the index
                            var ndx = db.Indices
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == shareId);


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


                                int saveResult = db.SaveChanges();
                                Console.WriteLine($"{saveResult} record saved.");
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
                        newChart.Name = $"{tidm.Replace(".LON", "")} Market RS ({newChart.BoxSize}, {reversal} rev)";
                    }
                    else if (chartSource == PnFChartSource.RSStockVSector)
                    {
                        newChart.Name = $"{tidm.Replace(".LON", "")} Peer RS ({newChart.BoxSize}, {reversal} rev)";
                    }
                    try
                    {
                        // Save the chart
                        using (var db = new PnFDataContext())
                        {
                            // Get the share
                            var share = db.Shares
                                .Include(sc => sc.Charts).ThenInclude(s => s.Chart)
                                .SingleOrDefault(s => s.Id == shareId);


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


                                int saveResult = db.SaveChanges();
                                Console.WriteLine($"{saveResult} record saved.");
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



        internal static void GenerateIndexPercentCharts()
        {
            List<PnFData.Model.Index> indices = null;
            try
            {
                using (PnFDataContext db = new PnFDataContext())
                {
                    indices = db.Indices.Where(s => s.IndexValues.Any()).ToList();
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
            else
            {
                Console.WriteLine($@"Tick data not available.");
                return;
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


                            int saveResult = db.SaveChanges();
                            Console.WriteLine($"{saveResult} record saved.");
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

        #region Helper methods ...

        private static DateTime PreviousWorkDay(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            } while (IsWeekend(date));

            return date;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }


        #endregion

        #region Testing long running calls ...
        private static readonly ManualResetEvent _reset = new ManualResetEvent(false);
        private static SqlParameter _returnParameter;

        internal static void RunLongStoredProcedure(string storedProcedure, int connectionTimeout)
        {
            _reset.Reset();
            try
            {
                using (SqlConnection conn = new SqlConnection(PnFDataContext.ConnectionString))
                using (SqlCommand command = new SqlCommand(storedProcedure, conn))
                {
                    _returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    _returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = connectionTimeout;   // Default to one minute
                    command.Connection.InfoMessage += ConnectionInfoMessage;
                    AsyncCallback runResult = new AsyncCallback(NonQueryCallBack);
                    command.Connection.Open();
                    command.BeginExecuteNonQuery(runResult, command);
                    Console.WriteLine($"Waiting for completion of {storedProcedure} ....");
                    _reset.WaitOne();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Problem with executing {storedProcedure}! - [{ ex.Message}]");
            }
            if (_LastReturnValue == 0)
            {
                Console.WriteLine($"{storedProcedure} Completed successfully");
            }
            else
            {
                Console.WriteLine($"Error! {storedProcedure} failed.");
            }
        }

        private static void ConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (e.Errors.Count > 0)
            {
                Console.WriteLine($"Received {e.Errors.Count} messages");
                foreach (SqlError info in e.Errors)
                {
                    if (info.Class > 9) // Severity
                    {
                        Console.WriteLine($"Error Message : {info.Message} : State : {info.State}");
                    }
                    else
                    {
                        Console.WriteLine(info.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void NonQueryCallBack(IAsyncResult result)
        {
            SqlCommand command = (SqlCommand)result.AsyncState;
            try
            {
                if (command != null)
                {
                    Console.WriteLine($"Waiting for completion of the Async call, result = {command.EndExecuteNonQuery(result)}");
                    Console.WriteLine($"ReturnParameter = {_returnParameter.Value}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Problem with executing command! - [{0}]", ex.Message);
            }
            finally
            {
                Console.WriteLine("Completed call back so signal main thread to continue....");
                _reset.Set();
            }
        }
        #endregion
    }
}
