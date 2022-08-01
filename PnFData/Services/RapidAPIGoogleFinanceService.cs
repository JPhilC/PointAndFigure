using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PnFData.Services.RapidAPIGoogleFinance
{
    public class GoogleTickerPriceResult
    {
        public bool InError { get; set; } = false;
        public string? Reason { get; set; }

        public double? Price { get; set; }

    }

    public class GoogleFinanceService
    {
        private static HttpClient _client;
        private static string ApiKey = "DEMO";  // This is now taken from the USER environment variable "RapidAPIKey"
        private static string RapidAPIHost = "google-finance4.p.rapidapi.com";
        private static readonly string ApiBaseUrl = @"https://google-finance4.p.rapidapi.com";

        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
                    _client.DefaultRequestHeaders.Add("X-RapidAPI-Key", ApiKey);
                    _client.DefaultRequestHeaders.Add("X-RapidAPI-Host", RapidAPIHost);
                }

                return _client;
            }
        }

        static GoogleFinanceService()
        {
            ApiKey = (string)Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User)["RapidAPIKey"];
        }

        public static async Task<GoogleTickerPriceResult> GetLastPrice(string symbol, string exchangeCode)
        {
            GoogleTickerPriceResult result = new GoogleTickerPriceResult();
            string convertedSymbol = symbol.Replace(".LON", "").Replace(".", "");
            string googleTicker = null;
            switch (exchangeCode)
            {
                case "LSE":
                    googleTicker = $"{convertedSymbol}%3ALON";
                    break;
                case "NYSE":
                    googleTicker = $"{convertedSymbol}%3ANYSE";
                    break;
                case "NASDAQ":
                    googleTicker = $"{convertedSymbol}%3ANASDAQ";
                    break;
            }
            if (googleTicker != null)
            {
                try
                {
                    string url = $"{ApiBaseUrl}/ticker/?t={googleTicker}&hl=en&gl=US";
                    HttpResponseMessage response = await Client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        {
                            Rootobject results = JsonConvert.DeserializeObject<Rootobject>(body);
                            if (results != null && results.price.last.value.HasValue)
                            {
                                result.InError = false;
                                result.Price = results.price.last.value.Value;
                            }
                            else
                            {
                                result.InError = true;
                                result.Reason = $"Price data is not availale for {symbol}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.InError = true;
                    result.Reason = $"Error processing TIDM: {symbol}, {ex.Message}";
                }

            }
            else
            {
                result.InError = true;
                result.Reason = $"Unrecognised exchange code '{exchangeCode}'";
            }
            return result;
        }


        public class Rootobject
        {
            public Info info { get; set; }
            public About about { get; set; }
            public Price price { get; set; }
            public Charts charts { get; set; }
            public News[] news { get; set; }
            public Stats stats { get; set; }
        }

        public class Info
        {
            public string type { get; set; }
            public string title { get; set; }
            public string ticker { get; set; }
            public string[] ticker_symbols { get; set; }
            public string country_code { get; set; }
        }

        public class About
        {
            public string symbol { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string about_url { get; set; }
            public Headquarters headquarters { get; set; }
            public Founded founded { get; set; }
            public string ceo { get; set; }
            public int? employees { get; set; }
            public string website { get; set; }
        }

        public class Headquarters
        {
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
            public string address { get; set; }
        }

        public class Founded
        {
            public int? year { get; set; }
            public int? month { get; set; }
            public int? day { get; set; }
        }

        public class Price
        {
            public string currency { get; set; }
            public float? previous_close { get; set; }
            public Last last { get; set; }
            public After_Market_Closing after_market_closing { get; set; }
        }

        public class Last
        {
            public float? value { get; set; }
            public float? today_change { get; set; }
            public float? today_change_percent { get; set; }
            public int? time { get; set; }
        }

        public class After_Market_Closing
        {
            public object value { get; set; }
            public object change { get; set; }
            public object change_percent { get; set; }
        }

        public class Charts
        {
            public _1Day[] _1day { get; set; }
            public _1Month[] _1month { get; set; }
        }

        public class _1Day
        {
            public string date { get; set; }
            public float? price { get; set; }
        }

        public class _1Month
        {
            public string date { get; set; }
            public float? price { get; set; }
        }

        public class Stats
        {
            public string currency { get; set; }
            public string day_range { get; set; }
            public string year_range { get; set; }
            public string market_cap { get; set; }
            public int? volume { get; set; }
            public float? pe_ratio { get; set; }
            public float? dividend_yield { get; set; }
            public string primary_market { get; set; }
        }

        public class News
        {
            public string url { get; set; }
            public string title { get; set; }
            public string thumbnail { get; set; }
            public int? published_time { get; set; }
            public Source source { get; set; }
        }

        public class Source
        {
            public string name { get; set; }
            public string logo { get; set; }
        }

    }
}
