using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace PnFConsole
{
    /**
 * 
 * this uses alphavantage.co quotes to put data into the StockData arrays.
 * 
 * you will need to register with alphavantage and get a key that must be added
 * to this code.
 * 
 * @author joe mcverry Change history:
 *
 *         YYMMDD BY Description 
 *         110103 JM Google logic. 
 *         110118 JM Adjust Yahoo  #'s with Adjusted Closing 
 *         120103 JM Changed Yahoo URL to  ichart.finance.yahoo... 
 *         120103 JM Changed default day request to  select last 360 days (orignally 1000). 
 *         120225 JM Removed prebuilt   array of data passing capabilities and cleaned up Google logic.
 *         190618 JM Removed code to get data from Google and Yahoo. 
 *         210123 JM  Added getting data from AlphaVantage.co Now using Java stream logic
 *         to build native arrays.
 *         210204 JM Removed AlphaVantage specific logic, stock price feed now
 *                       specified through properties file.
 * 
 * 
 */

    public class StockData : IStockData
    {

        private readonly double[] _opens;

        private readonly double[] _highs;

        private readonly double[] _lows;

        private readonly double[] _closes;

        private readonly double[] _volumes;

        private readonly string[] _dates;


        public StockData(double[] opens, double[] highs, double[] lows, double[] closes,
            double[] volumes, string[] dates)
        {
            this._opens = opens;
            this._highs = highs;
            this._lows = lows;
            this._closes = closes;
            this._volumes = volumes;
            this._dates = dates;
        }

        public static async Task<StockData> GetInstance(string sym)
        {
            return await AlphaVantageService.GetTimeSeriesDailyPrices(sym);
        }

        public double[] GetCloses()
        {
            return _closes;
        }


        public string[] GetDates()
        {
            return _dates;
        }


        public double[] GetHighs()
        {
            return _highs;
        }


        public double[] GetLows()
        {
            return _lows;
        }


        public double[] GetOpens()
        {
            return _opens;
        }


        public double[] GetVolumes()
        {
            return _volumes;
        }

        public bool IsGoodData =>
            this._opens.Length > 0 &&
            this._closes.Length == this._opens.Length &&
            this._highs.Length == this._opens.Length &&
            this._lows.Length == this._opens.Length;
    }
}
