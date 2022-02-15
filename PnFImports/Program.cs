using CsvHelper;
using CsvHelper.Configuration;
using PnFData.Model;
using PnFImports.Model;
using PnFImports.Services;
using System.Globalization;

namespace PnFImports
{
    internal class PnFImports
    {

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
                        PnFImports.ImportEodDailyPrices();
                        break;
                }
            }

            Console.WriteLine("Completed. Press a key to exit.");
            Console.ReadKey();

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
                var share = db.Shares.FirstOrDefault(b => b.ShareDataSource == "SS" & b.ShareDataSourceId == shareScopeId);
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
                }).OrderBy(s=>s.Tidm).ToList();
            }
            foreach (var shareData in shareIds)
            {
                if (shareData.EodErrors)
                {
                    continue;   // Skip as were no eod prices last time
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
                        var prices = await AlphaVantageService.GetTimeSeriesDailyPrices(shareData.Tidm, cutOffDate, true);
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

        internal static void ImportEodDailyPrices()
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
                Console.Write($"Processing {shareData.Tidm}: {shareData.Name} ...");
                if (shareData.EodErrors)
                {
                    Console.WriteLine("Skipped, previously in error!");
                    continue;   // Skip as were no eod prices last time
                }
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
                        var prices = await AlphaVantageService.GetTimeSeriesDailyPrices(shareData.Tidm, (shareData.LastEodDate ?? cutOffDate));
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

        #region Helper methods ...
        private static DateTime PreviousWorkDay(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            }
            while (IsWeekend(date));

            return date;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }
        #endregion

    }
}
