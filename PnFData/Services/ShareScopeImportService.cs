using CsvHelper;
using CsvHelper.Configuration;
using PnFData.Model;
using System.Globalization;

namespace PnFData.Services
{
    public static class ShareScopeImportService
    {
        public static (int, int, int) ImportShares(string filename)
        {
            Console.WriteLine("Importing Shares");
            int adds = 0;
            int updates = 0;
            int errors = 0;
            string exchange = null;
            using var db = new PnFDataContext();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(filename);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ShareScopeCompany>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            bool firstRecord = true;
            foreach (ShareScopeCompany data in records)
            {
                // If the first record then get the exchange coe
                if (firstRecord)
                {
                    exchange = data.ExchangeCode;
                    firstRecord = false;
                }
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
                }
            }
            Console.WriteLine($"Completed. {adds} new records added, {updates} records updated, {errors} errors.");
            return (adds, updates, errors);
        }

        public static (int, int, int) ImportETFs(string filename)
        {
            Console.WriteLine("Importing ETFs");
            int adds = 0;
            int updates = 0;
            int errors = 0;
            string exchange = null;
            using var db = new PnFDataContext();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(filename);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ShareScopeETF>();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            bool firstRecord = true;
            foreach (ShareScopeETF data in records)
            {
                if (firstRecord)
                {
                    // Get the exchange code
                    exchange = data.ExchangeCode;
                    firstRecord = false;
                }
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
                        SuperSector = "ETFs",
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
                    share.SuperSector = "ETFs";
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
                }
            }
            Console.WriteLine($"Completed. {adds} new records added, {updates} records updated, {errors} errors.");
            return (adds, updates, errors);
        }

    }
}
