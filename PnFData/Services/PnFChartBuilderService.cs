using PnFData.Model;

namespace PnFData.Services
{
    public class PnFChartBuilderService
    {

        private List<Eod> _eodList;

        public bool DontResize { get; set; } = true;

        public double BoxSize { get; set; } = 2.0d;

        public PnFChartBuilderService(List<Eod> eodList)
        {
            this._eodList = eodList;
        }


        public PnFChart BuildChart(double boxSize, int reversal)
        {
            PnFChart chart = new PnFChart()
            {
                BoxSize = boxSize,
                Reversal = reversal
            };
            List<Eod> sortedList = this._eodList.OrderBy(s => s.Day).ToList();
            bool firstEod = true;
            foreach (Eod eod in sortedList)
            {
                if (firstEod)
                {

                    firstEod = false;
                }
            }
            return chart;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double SetBoxSize(double value)
        {
            var boxSize = this.BoxSize;
            if (this.DontResize)
                return boxSize;

            boxSize = value switch
            {
                < 0.25 => .0625,
                < 1.00 => .125,
                < 5.00 => .25,
                < 20.00 => .50,
                < 100.00 => 1.00,
                < 200.00 => 2.00,
                < 500.00 => 4.00,
                < 1000.00 => 5.00,
                < 25000.00 => 50.00,
                _ => 500.00
            };

            return boxSize;

        }
    }
}
