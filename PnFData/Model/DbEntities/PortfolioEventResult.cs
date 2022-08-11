namespace PnFData.Model
{
    public class PortfolioEventResult
    {
        public string Tidm { get; set; }

        public string ShareName { get; set; }

        public string Portfolio { get; set; }

        public double Holding { get; set; }

        public double AdjustedClose { get; set;}

        public int NewEvents { get; set; }

        public string Remarks { get ; set; }
    }
}
