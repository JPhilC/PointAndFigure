namespace PnFConsole
{
    public interface IStockData
    {
        bool IsGoodData { get; }
        /** return all the closing prices */
        double[] GetCloses();

        /** return all the relative dates */
        string[] GetDates();

        /** return the highs */
        double[] GetHighs();

        /** return the lows */
        double[] GetLows();

        /** return the opening price */
        double[] GetOpens();

        /** return the volume numbers */
        double[] GetVolumes();

	}
}
