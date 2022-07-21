using CsvHelper.Configuration.Attributes;

namespace PnFData.Model
{
    public class ShareScopeETF
    {
        // TIDM	Name	Equity holdings	Net assets (share class)	ETF sector	Currency (prices)	Exchange code	ShareScope ID

        [Name("TIDM")]
        public string Tidm { get; set; }
        
        public string Name { get; set; }

        [Name("Equity holdings")]
        public int EquityHoldings { get; set; }
        
        [Name("Net assets (share class)")]
        public double NAVShareClass { get; set; }

        [Optional]
        [Name("ETF sector")]
        public string ETFSector { get; set; }

        [Name("Currency (prices)")]
        public string Currency { get; set; }
        
        [Name("Exchange code")]
        public string ExchangeCode { get; set; }

        [Name("ShareScope ID")]
        public int ShareScopeID { get; set; }

    }
}
