using CsvHelper;
using CsvHelper.Configuration;
using PnFData.Model;
using PnFData.Services;
using System.Data;
using System.Diagnostics;
using System.Globalization;


namespace PnFImports
{
    internal partial class PnFImports
    {
        internal static void ImportShares(string exchange)
        {
            Console.WriteLine("Importing Shares");
            int adds = 0;
            int updates = 0;
            int errors = 0;
            string filename = null;
            if (exchange == "LSE")
            {
                filename = @"C:\Users\phil\Downloads\Filtered_LSE shares_Company Export (LSE).csv";
            }
            else if (exchange == "NYSE")
            {
                filename = @"C:\Users\phil\Downloads\Filtered_NYSE_Company Export (NYSE).csv";
            }
            else if (exchange == "NASDAQ")
            {
                filename = @"C:\Users\phil\Downloads\Filtered_NASDAQ_Company Export (NASDAQ).csv";
            }
            else
            {
                Console.WriteLine($"Invalid exchange specified, '{exchange}");
                return;
            }
            using var db = new PnFDataContext();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(filename);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ShareScopeCompany>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (ShareScopeCompany data in records)
            {
                string tidm = data.Tidm;
                if (exchange == "LSE")
                {
                    tidm = data.Tidm + ".LON";
                }
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
                    adds++;
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
                    updates++;
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
                    errors++;
                    _LastReturnValue = 1;   // Signal an error.
                }
            }
            Console.WriteLine($"Completed. {adds} new records added, {updates} records updated, {errors} errors.");
        }

        internal static void ImportETFs(string exchange)
        {
            Console.WriteLine("Importing ETFs");
            int adds = 0;
            int updates = 0;
            int errors = 0;
            string filename = null;
            if (exchange == "LSE")
            {
                filename = @"C:\Users\phil\Downloads\Filtered_UK Exchange Traded Funds_ETFExport.csv";
            }
            else
            {
                Console.WriteLine($"Invalid exchange specified, '{exchange}");
                return;
            }
            using var db = new PnFDataContext();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(filename);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ShareScopeETF>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (ShareScopeETF data in records)
            {
                string tidm = data.Tidm;
                if (exchange == "LSE")
                {
                    tidm = data.Tidm + ".LON";
                }
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
                        ExchangeSubCode = "ETFs",
                        SharesInIssueMillions = data.EquityHoldings,
                        MarketCapMillions = data.NAVShareClass,
                        SuperSector = data.ETFSector,
                        Sector = data.ETFSector,
                        PricesCurrency = data.Currency,
                        ShareDataSource = "SS", // Share Scope
                        ShareDataSourceId = data.ShareScopeID.ToString() // Share Scope ID

                    }
                    );
                    adds++;
                }
                else
                {
                    Console.Write($"Updating {tidm}: {data.Name} ...");
                    share.Tidm = tidm;
                    share.Name = data.Name;
                    share.ExchangeCode = data.ExchangeCode;
                    share.ExchangeSubCode = "ETFs";
                    share.SharesInIssueMillions = data.EquityHoldings;
                    share.MarketCapMillions = data.NAVShareClass;
                    share.SuperSector = data.ETFSector;
                    share.Sector = data.ETFSector;
                    share.PricesCurrency = data.Currency;
                    db.Update(share);
                    updates++;
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
                    errors++;
                    _LastReturnValue = 1;   // Signal an error.
                }
            }
            Console.WriteLine($"Completed. {adds} new records added, {updates} records updated, {errors} errors.");
        }

        private static object _lock = new object();
        private static Stopwatch _stopwatch = new Stopwatch();
        private static long _nextSlot = 600;
        private static long _minInterval = 600;    // Milliseconds.
        internal static void ImportEodDailyPrices(string exchangeCode, string? tidm, bool retryErrors = false, bool fullImport = false)
        {
            try
            {
                DateTime cutOffDate = new(2005, 01, 01);
                List<ShareSummary> shareIds = new List<ShareSummary>();
                using (var db = new PnFDataContext())
                {
                    if (exchangeCode == "ALL")
                    {
                        shareIds = db.Shares
                            .Select(s => new ShareSummary()
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Tidm = s.Tidm,
                                EodErrors = s.EodError,
                                LastEodDate = s.LastEodDate,
                                HasPrices = s.EodPrices.Any()
                            })
                            .OrderBy(s => s.Tidm).ToList();
                        //.Where(s=> s.LastEodDate < new DateTime(2022, 05, 17))
                    }
                    else
                    {
                        shareIds = db.Shares.Where(s => s.ExchangeCode == exchangeCode)
                            .Select(s => new ShareSummary()
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Tidm = s.Tidm,
                                EodErrors = s.EodError,
                                LastEodDate = s.LastEodDate,
                                HasPrices = s.EodPrices.Any()
                            })
                            .OrderBy(s => s.Tidm).ToList();
                    }
                }

                _total = (double)shareIds.Count();
                _stopwatch.Start();
                _nextSlot = 0;
                _progress = 0;
                object errorlock = new object();
                Dictionary<string, string?> errors = new Dictionary<string, string?>();
                Parallel.ForEach(shareIds,
                    new ParallelOptions { MaxDegreeOfParallelism = 5 },
                    shareData =>
                    //foreach (var shareData in shareIds)
                    {
                        if (tidm != null && shareData.Tidm != tidm)
                        {
                            //continue;
                            return;
                        }
                        if (retryErrors)
                        {
                            if (!shareData.EodErrors)
                            {
                                return;
                            }
                        }

                        // Get the prices
                        Task.Run((async () =>
                        {
                            try
                            {
                                // Throttle to 150 calls per minute
                                lock (_lock)
                                {
                                    _stopwatch.Stop();
                                    long elapsed = _stopwatch.ElapsedMilliseconds;
                                    _stopwatch.Start();
                                    long delay = _nextSlot - elapsed;
                                    _nextSlot = elapsed + _minInterval;
                                    if (delay > 0)
                                    {
                                        Thread.Sleep((int)delay);
                                    }
                                }
                                using (var db = new PnFDataContext())
                                {
                                    var result = await AlphaVantageService.GetTimeSeriesDailyPrices(shareData.Tidm, (shareData.LastEodDate ?? cutOffDate), fullImport);
                                    Share share = db.Shares.First(s => s.Id == shareData.Id);
                                    if (result.InError)
                                    {
                                        share.EodError = true;
                                        db.Update(share);
                                        await db.SaveChangesAsync();
                                        lock (errorlock)
                                        {
                                            errors.Add(shareData.Tidm, result.Reason);
                                        }
                                        Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ...  Error! {result.Reason}");
                                    }
                                    else
                                    {
                                        var dayPrices = result.Prices as Eod[] ?? result.Prices.ToArray();
                                        if (dayPrices.Any())
                                        {
                                            foreach (Eod dayPrice in dayPrices)
                                            {
                                                dayPrice.ShareId = shareData.Id;
                                                db.EodPrices.Add(dayPrice);
                                            }
                                            DateTime maxDay = dayPrices.Max(p => p.Day);
                                            share.LastEodDate = maxDay;
                                            share.EodError = false;
                                            db.Update(share);

                                            await db.SaveChangesAsync();
                                            Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ...  OK.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ...  Skipped, up to date.");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (errorlock)
                                {
                                    errors.Add(shareData.Tidm, ex.Message);
                                }
                                Console.WriteLine(ex.ToString());
                            }
                        })).Wait();
                        // Indicate some form of progress
                        UpdateProgress();
                    });
                {

                }
                _stopwatch.Stop();

                if (errors.Any())
                {

                    string errorFile = Path.Combine(Path.GetTempPath(), $"PnFDesktopImport{(retryErrors?"_Retry":"")}.err");
                    using (StreamWriter writer = System.IO.File.CreateText(errorFile))
                    {
                        writer.WriteLine($"Import errors for: {DateTime.Now.ToShortDateString()}");
                        foreach(var error in errors)
                        {
                            writer.WriteLine($"{error.Key}\t{error.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _LastReturnValue = 1;
                Console.WriteLine("Error! Import Daily EOD prices failed.");
                Console.WriteLine(ex.Message);
            }

        }


    }
}
