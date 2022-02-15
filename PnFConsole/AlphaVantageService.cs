using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PnFConsole
{
    public class AlphaVantageService
    {
        private static HttpClient _client;
        private static readonly string ApiKey = "9XSPQTI5MMVQE5OK";
        private static readonly string ApiBaseUrl = @"https://www.alphavantage.co/query?";
        private static readonly string ApiFunction = "TIME_SERIES_DAILY";
        private static readonly string ApiOutputSize = "full";
        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
                }

                return _client;
            }
        }

        public static async Task<StockData> GetTimeSeriesDailyPrices(string symbol)
        {
            StockData data = null;
            List<double> op = new List<double>();
            List<double> hi = new List<double>();
            List<double> lo = new List<double>();
            List<double> cl = new List<double>();
            List<string> dt = new List<string>();
            List<double> vol = new List<double>();
            // List<double> adjClose = new List<double>();
            try
            {
                string url =
                    $"{ApiBaseUrl}function={ApiFunction}&symbol={symbol}&outputsize={ApiOutputSize}&apikey={ApiKey}";
                HttpResponseMessage response = await Client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string body = await response.Content.ReadAsStringAsync();
                    dynamic results = js.Deserialize(body, typeof(object));
                    if (results is Dictionary<string, object> timeSeriesDailyResult)
                    {
                        if (timeSeriesDailyResult["Time Series (Daily)"] is Dictionary<string, object> timeSeriesDaily)
                        {
                            foreach (var day in timeSeriesDaily)
                            {
                                string date = day.Key;
                                if (day.Value is Dictionary<string, object> prices)
                                {
                                    dt.Add(date);
                                    op.Add(double.Parse(prices["1. open"].ToString()));
                                    hi.Add(double.Parse(prices["2. high"].ToString()));
                                    lo.Add(double.Parse(prices["3. low"].ToString()));
                                    cl.Add(double.Parse(prices["4. close"].ToString()));
                                    vol.Add(double.Parse(prices["5. volume"].ToString()));
                                }
                            }
                        }

                        dt.Reverse();
                        op.Reverse();
                        hi.Reverse();
                        lo.Reverse();
                        cl.Reverse();
                        vol.Reverse();

                        data = new StockData(op.ToArray(),
                            hi.ToArray(),
                            lo.ToArray(),
                            cl.ToArray(),
                            vol.ToArray(),
                            dt.ToArray()
                        );
                    }
                    //if (results != null && results.Any())
                    //{
                    //    dayPrices = results.First().TimeSeriesDaily;
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return data;
        }

    }
}
