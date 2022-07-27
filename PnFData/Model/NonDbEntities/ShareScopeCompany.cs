using CsvHelper.Configuration.Attributes;

namespace PnFData.Model
{
    public class ShareScopeCompany
    {
        // TIDM,Name,Shares in issue (m),Market Cap. (intraday) (m),Supersector,Sector,Currency (prices),Exchange code,ShareScope ID,
        [Name("TIDM")]
        public string Tidm { get; set; }

        public string Name { get; set; }

        [Name("Shares in issue (m)")]
        public double SharesInIssue { get; set; }

        [Name("Market Cap. (m) (£)")]
        public double MarketCap { get; set; }

        [Optional]
        public string Supersector { get; set; }

        [Optional]
        public string Sector { get; set; }

        [Name("Currency (prices)")]
        public string Currency { get; set; }

        [Name("Exchange code")]
        public string ExchangeCode { get; set; }

        [Name("ShareScope ID")]
        public int ShareScopeID { get; set; }

        [Name("LSE listing")]
        public string ExchangeSubCode { get; set; }

        [Name("fc Yield")]
        public double? ForecastYield { get; set; }

        [Name("fc Yield %chg")]
        public double? ForecastYieldChange { get; set; }

        [Name("Dividend per share yrs pay Adj")]
        public double? DividendYearsPaid { get; set; }

        [Name("Dividend per share yrs gr Adj")]
        public double? DividendYearsGrowth { get; set; }

        [Name("ROCE")]
        public double? ROCE { get; set; }

        [Name("CROCI")]
        public double? CROCI { get; set; }

        [Name("EBIT margin")]
        public double? EBITMargin { get; set; }

        [Name("Free cash conversion")]
        public double? FreeCashConversion { get; set; }

        [Name("Fix. charge cover")]
        public double? FixChargeCover { get; set; }

        [Name("Debt to market cap.")]
        public double? DebtToMarketCap { get; set; }

        [Name("Pension to market cap.")]
        public double? PensionToMarketCap { get; set; }

        [Name("Beneish M-score")]
        public double? BeneishMScore { get; set; }

    }
}
