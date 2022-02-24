using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PnFData.Model;
using System.Diagnostics;
using System.Globalization;

namespace PnFImports.Services
{
    public class AlphaVantageService
    {
        private static HttpClient _client;
        private static readonly string ApiKey = "9XSPQTI5MMVQE5OK";
        private static readonly string ApiBaseUrl = @"https://www.alphavantage.co/query?";
        private static readonly string ApiFunction = "TIME_SERIES_DAILY";
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

        public static async Task<IEnumerable<Eod>> GetTimeSeriesDailyPrices(string symbol, DateTime cutoffDate, bool full = false)
        {
            string convertedSymbol = symbol.Replace("..", ".");
            string outputSize = full ? "full" : "compact";
            List<Eod> data = new();
            try
            {
                string url = $"{ApiBaseUrl}function={ApiFunction}&symbol={convertedSymbol}&outputsize={outputSize}&apikey={ApiKey}";
                HttpResponseMessage response = await Client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    {
                        JObject results = (JObject)JsonConvert.DeserializeObject(body)!;
                        IEnumerable<KeyValuePair<string, JToken?>> timeSeriesDaily = (IEnumerable<KeyValuePair<string, JToken?>>)results["Time Series (Daily)"]!;
                        foreach (var day in timeSeriesDaily)
                        {
                            string date = day.Key;
                            DateTime dateValue = DateTime.ParseExact(date, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None);
                            if (dateValue <= cutoffDate)
                            {
                                break; // Exist once the cut off date is reached.
                            }

                            IEnumerable<KeyValuePair<string, JToken>> prices = (IEnumerable<KeyValuePair<string, JToken>>)day.Value;
                            Eod eod = new Eod() { Day = dateValue };
                            foreach (var price in prices)
                            {
                                switch (price.Key)
                                {
                                    case "1. open":
                                        eod.Open = (double)price.Value;
                                        break;
                                    case "2. high":
                                        eod.High = (double)price.Value;
                                        break;
                                    case "3. low":
                                        eod.Low = (double)price.Value;
                                        break;
                                    case "4. close":
                                        eod.Close = (double)price.Value;
                                        break;
                                    case "5. volume":
                                        eod.Volume = (double)price.Value;
                                        break;
                                }

                            }
                            data.Add(eod);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing TIDM: {symbol}, {ex.Message}");
            }

            return data;
        }

    }
}
