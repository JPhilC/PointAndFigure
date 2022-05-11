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
        private static readonly string ApiKey = "demo";
        private static readonly string ApiBaseUrl = @"https://www.alphavantage.co/query?";
        private static readonly string ApiFunction = "TIME_SERIES_DAILY_ADJUSTED";
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

        public class TimeSeriesDailyResult
        {
            public bool InError { get; set; } = false;
            public string? Reason { get; set; }

            public IEnumerable<Eod> Prices = new List<Eod>();
        }

        public static async Task<TimeSeriesDailyResult> GetTimeSeriesDailyPrices(string symbol, DateTime cutoffDate, bool full = false)
        {
            string convertedSymbol = symbol.Replace("..", ".");
            string outputSize = full ? "full" : "compact";
            TimeSeriesDailyResult result = new TimeSeriesDailyResult();
            List<Eod> data = new List<Eod>();
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
                        if (timeSeriesDaily != null)
                        {
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
                                        case "5. adjusted close":
                                            eod.AdjustedClose = (double)price.Value;
                                            break;
                                        case "6. volume":
                                            eod.Volume = (double)price.Value;
                                            break;
                                        case "7. dividend amount":
                                            eod.DividendAmount = (double)price.Value;
                                            break;
                                        case "8. split coefficient":
                                            eod.SplitCoefficient = (double)price.Value;
                                            break;
                                    }

                                }
                                data.Add(eod);
                            }
                            result.Prices = data;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}");
                    result.InError = true;
                    result.Reason = response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                result.InError = true;
                result.Reason = $"Error processing TIDM: {symbol}, {ex.Message}";
            }

            return result;
        }

    }
}
