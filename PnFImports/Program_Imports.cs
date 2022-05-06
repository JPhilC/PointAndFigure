﻿using CsvHelper;
using CsvHelper.Configuration;
using PnFData.Model;
using PnFImports.Model;
using PnFImports.Services;
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

                    Thread.Sleep(400);
                })).Wait();
            }

        }

        private static object _lock = new object();
        private static Stopwatch _stopwatch = new Stopwatch();
        private static long _nextSlot = 400;
        private static long _minInterval = 400;    // Milliseconds.
        internal static void ImportEodDailyPrices(string exchangeCode, string? tidm)
        {
            try
            {
                DateTime cutOffDate = new(2016, 12, 31);
                DateTime lastClose = PreviousWorkDay(DateTime.Now.Date);
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

                _stopwatch.Start();
                _nextSlot = 0;
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
                        if (shareData.EodErrors)
                        {
                            Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ... Skipped, previously in error!");
                            // continue; // Skip as were no eod prices last time
                            return;
                        }

                        // Check if the current record is up to date.
                        if (shareData.LastEodDate >= lastClose)
                        {
                            Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ... Skipped, up to date.");
                            //continue;
                            return;
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
                                        Debug.WriteLine($"Sleeping {delay}");
                                        Thread.Sleep((int)delay);
                                    }
                                }
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
                                        Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ...  OK.");
                                    }
                                    else
                                    {
                                        share.EodError = true;
                                        db.Update(share);
                                        await db.SaveChangesAsync();
                                        Console.WriteLine($"Processing {shareData.Tidm}: {shareData.Name} ...  Error!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        })).Wait();
                    });
                _stopwatch.Stop();
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
