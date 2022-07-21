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
    }
}
