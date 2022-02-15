using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PnFConsole
{
    internal class PointAndFigure
    {
        private static StreamWriter _output;

        public static void Main()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("(quit to end) enter symbol: ");
                    string symb = Console.ReadLine()?.ToLower();
                    if (symb != null && symb.Length == 0) continue;
                    if (symb == "quit") return;
                    // GetETFData is configurable to select a specific # of days
                    IStockData gsd = null;
                    Task.WaitAll(Task.Run(async () =>
                   {
                       gsd = await StockData.GetInstance(symb);
                   }));
                    if (gsd != null && gsd.IsGoodData)
                    {
                        _output = new StreamWriter($"{symb}_PnF.txt");
                        PointAndFigure paf = new PointAndFigure(gsd);
                        SetToDontResize();
                        paf.SetDefaultBoxSize(gsd.GetLows()[0]);
                        // paf.ComputeBoxSize();
                        paf.ComputeUsingClosings();
                        // paf.ComputeUsingHighsAndLows();
                        paf.MakeCsvSimpleSpreadSheet(symb);
                        paf.Dump();
                        paf.GenericBuy();
                        paf.GenericSell();
                        foreach (string dt in paf.DateTypes.Keys)
                        {
                            _output.WriteLine($"{dt};{paf.DateTypes[dt].Mytype} {paf.DateTypes[dt].Text}");
                        }
                        _output.Flush();
                        _output.Close();
                        _output.Dispose();
                        _output = null;
                    }
                    else
                    {
                        Console.WriteLine($"Data not available for {symb}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
                finally
                {
                    if (_output != null)
                    {
                        _output.Dispose();
                    }
                }
            }
        }

        /** default reversal amount is 3 */
        private static readonly double ReversalAmount = 3;
        /** default box size is 1 */
        private static double _boxSize = 25.0;

        private static bool _dontResize;

        public static void SetToDontResize()
        {
            _dontResize = true;
        }

        private readonly double[] _lows;
        private readonly double[] _closings;
        private readonly double[] _highs;
        private readonly double[] _opens;
        private readonly double[] _volumes;
        private readonly string[] _dts;
        /** pafColumn is one column of x's or o's */
        private readonly List<PafColumn> _pafColumns;

        public class DateType
        {

            public Paftype Mytype;
            public string Text;

            public DateType(Paftype mytype, string text)
            {
                Mytype = mytype;
                Text = text;
            }

        }

        public Dictionary<string, DateType> DateTypes = new Dictionary<string, DateType>();

        public List<PafColumn> GetPafColumns()
        {
            return _pafColumns;
        }

        /**
		 * create a point and figure object with stock data
		 * 
		 * @param inGSD see StockData
		 */
        public PointAndFigure(IStockData inGsd)
        {

            _pafColumns = new List<PafColumn>();
            PafColumn.ResetLatestMonth();
            _lows = inGsd.GetLows();
            _closings = inGsd.GetCloses();
            _highs = inGsd.GetHighs();
            _opens = inGsd.GetOpens();
            _dts = inGsd.GetDates();
            _volumes = inGsd.GetVolumes();
            PafColumn.ResetLastCurrentBoxPosition();
            BuildBoxes();

        }

        /**
		 * create a point and figure object from data arrays
		 * 
		 * @param inLows
		 * @param inCloses
		 * @param inHighs
		 * @param inOpens
		 * @param inDates
		 * @param inVolumes
		 */
        public PointAndFigure(double[] inLows, double[] inCloses, double[] inHighs, double[] inOpens, string[] inDates, double[] inVolumes)
        {
            _pafColumns = new List<PafColumn>();
            PafColumn.ResetLatestMonth();
            _lows = inLows;
            _closings = inCloses;
            _highs = inHighs;
            _opens = inOpens;
            _dts = inDates;
            _volumes = inVolumes;
            PafColumn.ResetLastCurrentBoxPosition();
            BuildBoxes();

        }

        /** compute the box size based on the prices */
        public void ComputeBoxSize()
        {
            double bestLow = double.MaxValue;
            double bestHigh = double.MinValue;
            double sumHighLessLow = 0;
            int bi;
            for (bi = 0; bi < _lows.Length; bi++)
            {
                bestLow = Math.Min(bestLow, _lows[bi]);
                bestHigh = Math.Max(bestHigh, _highs[bi]);
                sumHighLessLow += (_highs[bi] - _lows[bi]);
            }

            _boxSize = (bestLow + bestHigh) / 2;
            int bs = (int)(_boxSize + .5);
            _boxSize = bs / 100d;
            _boxSize = sumHighLessLow / bi;
        }

        /**
		 * get the box size based on the low value passed, i forget where i found this
		 * criteria
		 * 
		 * @param inLow
		 * @return
		 */
        public double SetDefaultBoxSize(double inLow)
        {

            if (_dontResize)
                return _boxSize;

            if (inLow < 0.25)
                _boxSize = .0625;
            else if (inLow < 1.00)
                _boxSize = .125;
            else if (inLow < 5.00)
                _boxSize = .25;
            else if (inLow < 20.00)
                _boxSize = .50;
            else if (inLow < 100.00)
                _boxSize = 1.00;
            else if (inLow < 200.00)
                _boxSize = 2.00;
            else if (inLow < 500.00)
                _boxSize = 4.00;
            else if (inLow < 1000.00)
                _boxSize = 5.00;
            else if (inLow < 25000.00)
                _boxSize = 50.00;
            else
                _boxSize = 500.00;

            return _boxSize;
        }

        /**
		 * do paf calculations based on the high and low of the date
		 */
        public void ComputeUsingHighsAndLows()
        {

            PafColumn currentColumn;
            if (_opens[0] > _closings[0])
            { // seed first box as an O, for today was a
              // down day
                currentColumn = new PafColumn(Paftype.O, _highs[0], _dts[0], _boxes);
                currentColumn.AddToColumn(_lows[0], _dts[0]);
            }
            else
            { // or seed first box as an X
                currentColumn = new PafColumn(Paftype.X, _lows[0], _dts[0], _boxes);
                currentColumn.AddToColumn(_highs[0], _dts[0]);
            }
            currentColumn.Volume = _volumes[0];

            _pafColumns.Add(currentColumn);
            for (int i = 1; i < _highs.Length; i++)
            {
                SetDefaultBoxSize(_lows[i]);
                if (currentColumn.MyType == Paftype.X)
                {
                    if (_highs[i] >= (currentColumn.GetCurrentBox() + (_boxSize * ReversalAmount)))
                    {
                        currentColumn.AddToColumn(_highs[i], _dts[i]);
                    }
                    else if (_lows[i] <= (currentColumn.GetCurrentBox() - (_boxSize * ReversalAmount)))
                    {
                        currentColumn = new PafColumn(Paftype.O, _lows[i], _dts[i], _boxes);
                        _pafColumns.Add(currentColumn);
                    }
                } // if type.X

                if (currentColumn.MyType == Paftype.O)
                {
                    if (_lows[i] <= (currentColumn.GetCurrentBox() - (_boxSize * ReversalAmount)))
                    {
                        currentColumn.AddToColumn(_lows[i], _dts[i]);
                    }
                    else if (_highs[i] >= (currentColumn.GetCurrentBox() + (_boxSize * ReversalAmount)))
                    {
                        currentColumn = new PafColumn(Paftype.X, _highs[i], _dts[i], _boxes);
                        _pafColumns.Add(currentColumn);
                    }
                } // if type.O
                currentColumn.Volume += _volumes[i];

            }

        }

        /**
		 * do paf calculations based on the closing price
		 */
        public void ComputeUsingClosings()
        {

            PafColumn currentColumn;
            if (_opens[0] > _closings[0])
            { // seed first box as an O
                currentColumn = new PafColumn(Paftype.O, _highs[0], _dts[0], _boxes);
                currentColumn.AddToColumn(_closings[0], _dts[0]);
            }
            else
            { // or seed first box as an X
                currentColumn = new PafColumn(Paftype.X, _opens[0], _dts[0], _boxes);
                currentColumn.AddToColumn(_closings[0], _dts[0]);
            }

            currentColumn.Volume = _volumes[0];

            _pafColumns.Add(currentColumn);
            for (int i = 1; i < _highs.Length; i++)
            {
                SetDefaultBoxSize(_closings[i]);
                if (currentColumn.MyType == Paftype.O)
                {
                    if (_closings[i] >= (currentColumn.GetCurrentBox() + (_boxSize * ReversalAmount)))
                    {
                        currentColumn = new PafColumn(Paftype.X, _closings[i], _dts[i], _boxes);
                        _pafColumns.Add(currentColumn);
                    }
                    else
                        currentColumn.AddToColumn(_closings[i], _dts[i]);
                    currentColumn.Volume += _volumes[i];

                } // if type.O

                else
                {
                    if (_closings[i] <= (currentColumn.GetCurrentBox() - (_boxSize * ReversalAmount)))
                    {
                        currentColumn = new PafColumn(Paftype.O, _closings[i], _dts[i], _boxes);
                        _pafColumns.Add(currentColumn);
                    }
                    else
                        currentColumn.AddToColumn(_closings[i], _dts[i]);
                    currentColumn.Volume += _volumes[i];
                }
                DateType dateType;
                if (currentColumn.MyType == Paftype.O)
                {
                    dateType = new DateType(currentColumn.MyType, GenericSellString());
                }
                else
                {
                    dateType = new DateType(currentColumn.MyType, GenericBuyString());
                }

                DateTypes[_dts[i]] = dateType;
            }
        }

        /**
		 * generates a csv file for import to a spreadsheet to generate a rudimentary
		 * chart, writes to System Out
		 */
        public void MakeCsvSimpleSpreadSheet(string symbol)
        {
            using (StreamWriter sw = new StreamWriter($"{symbol}_chart.csv")) {
                for (int boxpos = _boxes.Length - 1; boxpos > 0; boxpos--)
                {
                    sw.Write(_boxes[boxpos]);
                    for (int column = 0; column < _pafColumns.Count; column++)
                    {
                        PafColumn currentColumn = _pafColumns[column];
                        if (currentColumn.MyType == Paftype.O)
                        {
                            if (boxpos < currentColumn.StopAt)
                            {
                                sw.Write(",");
                            }
                            else if (boxpos <= currentColumn.StartAt)
                            {
                                if (currentColumn.MonthIndicators.ContainsKey(boxpos))
                                {
                                    sw.Write("," + currentColumn.MonthIndicators[boxpos]);
                                }
                                else
                                    sw.Write("," + currentColumn.MyType);
                            }
                            else
                            {
                                sw.Write(",");
                            }
                        }
                        else
                        {
                            if (boxpos < currentColumn.StartAt)
                            {
                                sw.Write(",");
                            }
                            else if (boxpos <= currentColumn.StopAt)
                            {
                                if (currentColumn.MonthIndicators.ContainsKey(boxpos))
                                {
                                    sw.Write("," + currentColumn.MonthIndicators[boxpos]);
                                }
                                else
                                    sw.Write("," + currentColumn.MyType);
                            }
                            else
                            {
                                sw.Write(",");
                            }
                        }
                    }
                    sw.WriteLine("," + _boxes[boxpos]);
                }
                string lastYear = "";
                bool[] dontShowYears = new bool[_pafColumns.Count];
                for (int column = 0; column < _pafColumns.Count; column++)
                {
                    PafColumn currentColumn = _pafColumns[column];
                    string thisYear = "" + currentColumn.StartDate[2] + currentColumn.StartDate[3];
                    dontShowYears[column] = (lastYear==thisYear);
                    lastYear = thisYear;
                }
                int[] dateFields = { 2, 3 };
                for (int showField = 0; showField < dateFields.Length; showField++)
                {
                    sw.Write(",");
                    for (int column = 0; column < _pafColumns.Count; column++)
                    {
                        PafColumn currentColumn = _pafColumns[column];
                        if (dontShowYears[column])
                            sw.Write(",");

                        else if (showField == 0 && currentColumn.StartDate[dateFields[showField]] == '0')
                            sw.Write(",");

                        else
                            sw.Write(currentColumn.StartDate[dateFields[showField]] + ",");
                    }
                    sw.WriteLine("");
                }
            }
        }

        // P & F Buy
        // P & F Sell
        // double Top Breakout
        // double Bottom Breakdown
        // Triple Top Breakout
        // Triple Bottom Breakdown
        // Quadruple Top Breakout
        // Quadruple Bottom Breakdown
        // Ascending Triple Top Breakout
        // Descending Triple Bottom Breakdown
        // Bullish Catapult Breakout
        // Bearish Catapult Breakdown
        // Bullish Signal Reversed
        // Bearish Signal Reversed
        // Bullish Triangle Breakout
        // Bearish Triangle Breakdown
        // Long Tail Down Reversal
        // Bull Trap
        // Bear Trap
        // Spread Triple Top
        // Spread Triple Bottom Breakdown
        // High Pole
        // Low Pole Reversal

        public static string BuySignal = "Buy Signal";
        public static string SellSignal = "Sell Signal";

        public string GetPandFBuy()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox())
                        return BuySignal + " at " + _pafColumns[colCheck];
                }
            }
            return "";
        }

        public string GetPandFSell()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox())
                        return SellSignal + " at " + _pafColumns[colCheck];
                }
            }
            return "";
        }

        public static string DoubleTop = "double Top";
        public static string DoubleTopBreakout = "double Top Breakout";

        public string GetDoubleTopAndBreakouts()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 2)
                {
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance)
                    {
                        return DoubleTop + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2];

                    }
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        return DoubleTopBreakout + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2];
                    }
                }
            }
            return "";

        }

        public static string DoubleBottom = "double Bottom";
        public static string DoubleBottomBreakdown = "double Bottom Breakdown";

        public string GetDoubleBottomAndBreakdowns()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {

                if (colSize > 3)
                {
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance)
                    {
                        return DoubleBottom + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2];
                    }
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        return DoubleBottomBreakdown + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2];
                    }
                }
            }
            return "";

        }

        public static string TripleTop = "Triple Top";
        public static string TripleTopBreakout = "Triple Top Breakout";

        public string GetTripleTopAndBreakouts()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 4)
                {
                    // Triple Top and Breakouts
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return TripleTop + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4];
                    }
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return TripleTopBreakout + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4];
                    }
                }
            }
            return "";
        }

        public static string TripleBottom = "Triple Bottom";
        public static string TripleBottomBreakdown = "Triple Bottom Breakdown";

        public string GetTripleBottomAndBreakdowns()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {
                if (colSize > 4)
                {
                    // Triple Bottom and Breakdowns
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return TripleBottom + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4];
                    }
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return TripleBottomBreakdown + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4];
                    }
                }
            }
            return "";

        }

        public static string QuadrupleTop = "Quadruple Top";
        public static string QuadrupleTopBreakout = "Quadruple Top Breakout";

        public string GetQuadrupleTopAndBreakouts()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 6)
                {
                    // Quadruple Top and Breakouts
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return QuadrupleTop + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4] + " and "
                                + _pafColumns[colCheck - 6];

                    }
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return QuadrupleTopBreakout + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4] + " and "
                                + _pafColumns[colCheck - 6];
                    }
                }
            }
            return "";
        }

        public static string QuadrupleBottom = "Quadruple Bottom";
        public static string QuadrupleBottomBreakdown = "Quadruple Bottom Breakdown";

        public string GetQuadrupleBottomAndBreakdowns()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {
                if (colSize > 6)
                {
                    // Quadruple Bottom and Breakdowns
                    if (Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 2].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return QuadrupleBottom + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4] + " and "
                                + _pafColumns[colCheck - 6];

                    }
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck - 2].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return QuadrupleBottomBreakdown + " at " + _pafColumns[colCheck] + " and "
                                + _pafColumns[colCheck - 2] + " and "
                                + _pafColumns[colCheck - 4] + " and "
                                + _pafColumns[colCheck - 6];
                    }
                }
            }
            return "";
        }

        public static string AscendingTripleBreakout = "Ascending Triple Breakout";

        public string GetAscendingTripleBreakout()
        {

            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        if (colSize > 4)
                        {
                            if (_pafColumns[colCheck - 2].GetCurrentBox() > _pafColumns[colCheck - 4]
                                    .GetCurrentBox())
                            {

                                return AscendingTripleBreakout + " at " + _pafColumns[colCheck] + " and "
                                        + _pafColumns[colCheck - 2] + " and "
                                        + _pafColumns[colCheck - 4];
                            }
                        }
                    }
                }
            }
            return "";
        }

        public static string DescendingTripleBreakdown = "Descending Triple Breakdown";

        public string GetDescendingTripleBreakdown()
        {

            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        if (colSize > 4)
                        {
                            if (_pafColumns[colCheck - 2].GetCurrentBox() < _pafColumns[colCheck - 4]
                                    .GetCurrentBox())
                            {

                                return DescendingTripleBreakdown + " at " + _pafColumns[colCheck] + " and "
                                        + _pafColumns[colCheck - 2] + " and "
                                        + _pafColumns[colCheck - 4];
                            }
                        }
                    }
                }
            }
            return "";
        }

        public static string BullishCatapultBreakout = "Bullish Catapult Breakout";

        public string GetBullishCatapultBreakout()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        if (colSize > 4)
                        {
                            if (_pafColumns[colCheck - 2].GetCurrentBox() > _pafColumns[colCheck - 4]
                                    .GetCurrentBox())
                            {
                                if (colSize > 6)
                                {
                                    if (Math.Abs(_pafColumns[colCheck - 4].GetCurrentBox() - _pafColumns[colCheck - 6]
                                            .GetCurrentBox()) < Tolerance)
                                    {

                                        return BullishCatapultBreakout + " at " + _pafColumns[colCheck]
                                                + " and " + _pafColumns[colCheck - 2] + " and "
                                                + _pafColumns[colCheck - 4] + " and "
                                                + _pafColumns[colCheck - 6];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "";

        }

        public static string BearishCatapultBreakdown = "Bearish Catapult Breakdown";

        public string GetBearishCatapultBreakdown()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.O)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox())
                    {
                        if (colSize > 4)
                        {
                            if (_pafColumns[colCheck - 2].GetCurrentBox() < _pafColumns[colCheck - 4]
                                    .GetCurrentBox())
                            {
                                if (colSize > 6)
                                {
                                    if (Math.Abs(_pafColumns[colCheck - 4].GetCurrentBox() - _pafColumns[colCheck - 6]
                                            .GetCurrentBox()) < Tolerance)
                                    {

                                        return BearishCatapultBreakdown + " at " + _pafColumns[colCheck]
                                                + " and " + _pafColumns[colCheck - 2] + " and "
                                                + _pafColumns[colCheck - 4] + " and "
                                                + _pafColumns[colCheck - 6];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }

        public static string BullishTriangleBreakout = "Bullish Triangle Breakout";

        public string GetBullishTriangleBreakout()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colSize > 6)
            {
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox() // rising X
                            && _pafColumns[colCheck - 1].GetCurrentBox() > _pafColumns[colCheck - 3].GetCurrentBox() // rising
                                                                                                                   // o
                            && _pafColumns[colCheck - 2].GetCurrentBox() < _pafColumns[colCheck - 4].GetCurrentBox() // falling
                                                                                                                   // x
                            && _pafColumns[colCheck - 3].GetCurrentBox() > _pafColumns[colCheck - 5].GetCurrentBox() // rising
                                                                                                                   // o
                            && _pafColumns[colCheck - 4].GetCurrentBox() < _pafColumns[colCheck - 6].GetCurrentBox()) // falling
                                                                                                                    // x
                        return BullishTriangleBreakout + " at " + _pafColumns[colCheck];
                }

            }
            return "";
        }

        public static string BearishTriangleBreakdown = "Bearish Triangle Breakdown";

        public string GetBearishTriangleBreakdown()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colSize > 6)
            {
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox() // falling o
                            && _pafColumns[colCheck - 1].GetCurrentBox() < _pafColumns[colCheck - 3].GetCurrentBox() // falling
                                                                                                                   // x
                            && _pafColumns[colCheck - 2].GetCurrentBox() > _pafColumns[colCheck - 4].GetCurrentBox() // rising
                                                                                                                   // o
                            && _pafColumns[colCheck - 3].GetCurrentBox() < _pafColumns[colCheck - 5].GetCurrentBox() // falling
                                                                                                                   // x
                            && _pafColumns[colCheck - 4].GetCurrentBox() > _pafColumns[colCheck - 6].GetCurrentBox()) // rising
                                                                                                                    // o
                        return BearishTriangleBreakdown + " at " + _pafColumns[colCheck];
                }

            }
            return "";
        }

        public static string BullishSignalReversed = "Bullish Signal Reversed";

        public string GetBullishSignalReversed()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colSize > 7)
            {
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox() // falling o
                            && _pafColumns[colCheck - 1].GetCurrentBox() > _pafColumns[colCheck - 3].GetCurrentBox() // rising
                                                                                                                   // X
                            && _pafColumns[colCheck - 2].GetCurrentBox() > _pafColumns[colCheck - 4].GetCurrentBox() // rising
                                                                                                                   // o
                            && _pafColumns[colCheck - 3].GetCurrentBox() > _pafColumns[colCheck - 5].GetCurrentBox() // rising
                                                                                                                   // X
                            && _pafColumns[colCheck - 4].GetCurrentBox() > _pafColumns[colCheck - 6].GetCurrentBox() // rising
                                                                                                                   // o
                            && _pafColumns[colCheck - 5].GetCurrentBox() > _pafColumns[colCheck - 7].GetCurrentBox()) // rising
                                                                                                                    // X
                        return BullishSignalReversed + " at " + _pafColumns[colCheck];
                }
            }
            return "";
        }

        public static string BearishSignalReversed = "Bearish Signal Reversed";

        public string GetBearishSignalReversed()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colSize > 7)
            {
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox() // rising x
                            && _pafColumns[colCheck - 1].GetCurrentBox() < _pafColumns[colCheck - 3].GetCurrentBox() // falling
                                                                                                                   // o
                            && _pafColumns[colCheck - 2].GetCurrentBox() < _pafColumns[colCheck - 4].GetCurrentBox() // falling
                                                                                                                   // x
                            && _pafColumns[colCheck - 3].GetCurrentBox() < _pafColumns[colCheck - 5].GetCurrentBox() // falling
                                                                                                                   // o
                            && _pafColumns[colCheck - 4].GetCurrentBox() < _pafColumns[colCheck - 6].GetCurrentBox() // falling
                                                                                                                   // x
                            && _pafColumns[colCheck - 5].GetCurrentBox() < _pafColumns[colCheck - 7].GetCurrentBox()) // falling
                                                                                                                    // o
                        return BearishSignalReversed + " at " + _pafColumns[colCheck];
                }

            }
            return "";
        }

        public static string LongTailDownReversal = "Long tail down reversal";

        public string GetLongTailDownReversal()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (_pafColumns[colCheck].MyType == Paftype.X)
            {
                if (colSize > 2)
                {
                    if (_pafColumns[colCheck - 1].StartAt - _pafColumns[colCheck - 1].StopAt > 19)
                    {
                        return LongTailDownReversal + " at " + _pafColumns[colCheck];
                    }
                }
            }
            return "";
        }

        public static string BullTrap = "Bull Trap";

        public string GetBullTrap()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 5)
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck - 1].StopAt == _pafColumns[colCheck - 3].StopAt + 1 // only one box
                                                                                               // spread
                            && Math.Abs(_pafColumns[colCheck - 3].GetCurrentBox() - _pafColumns[colCheck - 5]
                                                                                                   .GetCurrentBox()) < Tolerance)
                    {
                        return BullTrap + " at " + _pafColumns[colCheck];
                    }
                }

            return "";
        }

        public static string BearTrap = "Bear Trap";

        public string GetBearTrap()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 5)
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {
                    if (_pafColumns[colCheck - 1].StopAt == _pafColumns[colCheck - 3].StopAt - 1 // only one box
                                                                                               // spread
                            && Math.Abs(_pafColumns[colCheck - 3].GetCurrentBox() - _pafColumns[colCheck - 5]
                                                                                                   .GetCurrentBox()) < Tolerance)
                    {
                        return BearTrap + " at " + _pafColumns[colCheck];
                    }
                }

            return "";
        }

        public static string SpreadTripleTop = "Spread Triple Top";

        public string GetSpreadTripleTop()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 5)
            {
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 6].GetCurrentBox()) < Tolerance)
                    {
                        return SpreadTripleTop;
                    }
                }
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 2].GetCurrentBox()
                            && _pafColumns[colCheck].GetCurrentBox() > _pafColumns[colCheck - 4].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 4].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return SpreadTripleTop + " Breakout";
                    }
                }
            }

            return "";
        }

        public static string SpreadTripleBottom = "Spread Triple Bottom ";

        public string GetSpreadTripleBottom()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 5)
            {
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 4].GetCurrentBox()) < Tolerance
                            && Math.Abs(_pafColumns[colCheck].GetCurrentBox() - _pafColumns[colCheck - 6].GetCurrentBox()) < Tolerance)
                    {
                        return SpreadTripleBottom;
                    }
                }
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 2].GetCurrentBox()
                            && _pafColumns[colCheck].GetCurrentBox() < _pafColumns[colCheck - 4].GetCurrentBox()
                            && Math.Abs(_pafColumns[colCheck - 4].GetCurrentBox() - _pafColumns[colCheck - 6]
                                .GetCurrentBox()) < Tolerance)
                    {
                        return SpreadTripleBottom + " Breakdown";
                    }
                }
            }

            return "";
        }

        public static string HighPole = "High Pole";

        public string GetHighPole()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 2)
            {
                if (_pafColumns[colCheck].MyType == Paftype.O)
                {
                    if (_pafColumns[colCheck - 1].StopAt > (_pafColumns[colCheck - 3].StopAt + 3)
                            && ((_pafColumns[colCheck].StartAt
                                    - _pafColumns[colCheck].StopAt) >= (_pafColumns[colCheck - 1].StopAt
                                            - _pafColumns[colCheck - 1].StartAt) / 2))
                        return HighPole;
                }
            }

            return "";
        }

        public static string LowPoleReversal = "Low Pole Reversal";

        public string GetLowPoleReversal()
        {
            int colSize = _pafColumns.Count;
            int colCheck = colSize - 1;
            if (colCheck > 2)
            {
                if (_pafColumns[colCheck].MyType == Paftype.X)
                {

                    if (_pafColumns[colCheck - 1].StopAt < (_pafColumns[colCheck - 3].StopAt - 3)
                            && ((_pafColumns[colCheck].StopAt
                                    - _pafColumns[colCheck].StartAt) <= (_pafColumns[colCheck - 1].StartAt
                                            - _pafColumns[colCheck - 1].StopAt) / 2))
                        return LowPoleReversal;
                }
            }

            return "";
        }

        public void DumpPrint(string doWePrintIt)
        {
            if (doWePrintIt.Length > 0)
                _output.WriteLine(doWePrintIt);
        }

        /**
		 * dump the paf objects to a readable format
		 */

        public void Dump()
        {
            for (int i = 0; i < _pafColumns.Count; i++)
            {
                _output.WriteLine(_pafColumns[i].ToString());
            }

            SupportLevels lSupport = new SupportLevels(LevelType.Support, _pafColumns);
            _output.WriteLine(lSupport.ToString());

            _output.WriteLine("Box size is " + _boxSize);

            SupportLevels lResistance = new SupportLevels(LevelType.Resistance, _pafColumns);
            _output.WriteLine(lResistance.ToString());
            
            _output.WriteLine("==== GetPandFBuy()");
            DumpPrint(GetPandFBuy());

            _output.WriteLine("==== GetPandFSell()");
            DumpPrint(GetPandFSell());

            _output.WriteLine("==== GetDoubleTopAndBreakouts()");
            DumpPrint(GetDoubleTopAndBreakouts());

            _output.WriteLine("==== GetAscendingTripleBreakout()");
            DumpPrint(GetAscendingTripleBreakout());

            _output.WriteLine("==== GetDoubleBottomAndBreakdowns()");
            DumpPrint(GetDoubleBottomAndBreakdowns());

            _output.WriteLine("==== GetTripleTopAndBreakouts()");
            DumpPrint(GetTripleTopAndBreakouts());

            _output.WriteLine("==== GetTripleBottomAndBreakdowns()");
            DumpPrint(GetTripleBottomAndBreakdowns());

            _output.WriteLine("==== GetQuadrupleTopAndBreakouts()");
            DumpPrint(GetQuadrupleTopAndBreakouts());

            _output.WriteLine("==== GetQuadrupleBottomAndBreakdowns()");
            DumpPrint(GetQuadrupleBottomAndBreakdowns());

            _output.WriteLine("==== GetAscendingTripleBreakout()");
            DumpPrint(GetAscendingTripleBreakout());

            _output.WriteLine("==== GetDescendingTripleBreakdown()");
            DumpPrint(GetDescendingTripleBreakdown());

            _output.WriteLine("==== GetBullishCatapultBreakout()");
            DumpPrint(GetBullishCatapultBreakout());

            _output.WriteLine("==== GetBearishCatapultBreakdown()");
            DumpPrint(GetBearishCatapultBreakdown());

            _output.WriteLine("==== GetBullishSignalReversed()");
            DumpPrint(GetBullishSignalReversed());

            _output.WriteLine("==== GetBearishSignalReversed()");
            DumpPrint(GetBearishSignalReversed());

            _output.WriteLine("==== GetBullishTriangleBreakout()");
            DumpPrint(GetBullishTriangleBreakout());

            _output.WriteLine("==== GetBearishTriangleBreakdown()");
            DumpPrint(GetBearishTriangleBreakdown());

            _output.WriteLine("==== GetLongTailDownReversal()");
            DumpPrint(GetLongTailDownReversal());

            _output.WriteLine("==== GetBullTrap()");
            DumpPrint(GetBullTrap());

            _output.WriteLine("==== GetBearTrap()");
            DumpPrint(GetBearTrap());

            _output.WriteLine("==== GetSpreadTripleTop()");
            DumpPrint(GetSpreadTripleTop());

            _output.WriteLine("==== GetSpreadTripleBottom()");
            DumpPrint(GetSpreadTripleBottom());

            _output.WriteLine("==== GetHighPole()");
            DumpPrint(GetHighPole());

            _output.WriteLine("==== GetLowPoleReversal()");
            DumpPrint(GetLowPoleReversal());

        }

        private double[] _boxes;

        public static double Tolerance { get; private set; } = 0.0001d;

        /** build a column of boxes that indicate the starting point of each box. */
        public void BuildBoxes()
        {
            double bestLow = double.MaxValue;
            double bestHigh = double.MinValue;
            int bi;
            for (bi = 0; bi < _lows.Length; bi++)
            {
                bestLow = Math.Min(bestLow, _lows[bi]);
                bestHigh = Math.Max(bestHigh, _highs[bi]);
            }

            bestLow -= .1;
            bestHigh += .1;
            double ds = (bestLow - SetDefaultBoxSize(bestLow));
            double intv = (int)ds;

            List<double> dboxes = new List<double>();
            for (double l = intv; l < bestHigh + _boxSize * 2; l += SetDefaultBoxSize(l))
            {
                dboxes.Add(l);
            }

            _boxes = new double[dboxes.Count + 1];
            for (int i = 0; i < dboxes.Count; i++)
            {
                _boxes[i] = dboxes[i];
            }
            _boxes[dboxes.Count] = _boxes[dboxes.Count - 1] + _boxSize;

        }

        public bool GenericBuy()
        {

            bool ret = false;
            ret |= GetPandFBuy().Length > 0;
            ret |= GetBearTrap().Length > 0;
            ret |= GetBullishCatapultBreakout().Length > 0;
            ret |= GetBearishSignalReversed().Length > 0;
            ret |= GetBullishTriangleBreakout().Length > 0;
            ret |= GetAscendingTripleBreakout().Length > 0;
            ret |= GetDoubleTopAndBreakouts().Length > 0;
            ret |= GetTripleTopAndBreakouts().Length > 0;
            ret |= GetQuadrupleTopAndBreakouts().Length > 0;
            ret |= GetSpreadTripleTop().Length > 0;
            return ret;
        }

        public string GenericBuyString()
        {

            if (GetQuadrupleTopAndBreakouts().Length > 0)
                return "quadrupleTopAndBreakouts";
            if (GetAscendingTripleBreakout().Length > 0)
                return "ascendingTripleBreakout";
            if (GetSpreadTripleTop().Length > 0)
                return "spreadTripleTop";
            if (GetTripleTopAndBreakouts().Length > 0)
                return "tripleTopAndBreakouts";
            if (GetDoubleTopAndBreakouts().Length > 0)
                return "doubleTopAndBreakouts";
            if (GetBullishTriangleBreakout().Length > 0)
                return "bullishTriangleBreakout";
            if (GetBearishSignalReversed().Length > 0)
                return "bearishSignalReversed";
            if (GetBullishCatapultBreakout().Length > 0)
                return "bullishCatapultBreakout";
            if (GetBearTrap().Length > 0)
                return "bearTrap";
            if (GetPandFBuy().Length > 0)
                return "buy";
            return "";
        }

        public int GenericBuyCount()
        {

            int cnt = 0;
            if (GetPandFBuy().Length > 0)
                cnt++;
            if (GetBearTrap().Length > 0)
                cnt++;
            if (GetBullishCatapultBreakout().Length > 0)
                cnt++;
            if (GetBearishSignalReversed().Length > 0)
                cnt++;
            if (GetBullishTriangleBreakout().Length > 0)
                cnt++;
            if (GetAscendingTripleBreakout().Length > 0)
                cnt++;
            if (GetDoubleTopAndBreakouts().Length > 0)
                cnt++;
            if (GetTripleTopAndBreakouts().Length > 0)
                cnt++;
            if (GetQuadrupleTopAndBreakouts().Length > 0)
                cnt++;
            if (GetSpreadTripleTop().Length > 0)
                cnt++;
            return cnt;
        }

        public bool GenericSell()
        {

            bool ret = false;
            ret |= GetPandFSell().Length > 0;
            ret |= GetBullTrap().Length > 0;
            ret |= GetBearishCatapultBreakdown().Length > 0;
            ret |= GetBullishSignalReversed().Length > 0;
            ret |= GetBearishTriangleBreakdown().Length > 0;
            ret |= GetDescendingTripleBreakdown().Length > 0;
            ret |= GetDoubleBottomAndBreakdowns().Length > 0;
            ret |= GetTripleBottomAndBreakdowns().Length > 0;
            ret |= GetQuadrupleBottomAndBreakdowns().Length > 0;
            ret |= GetSpreadTripleBottom().Length > 0;
            return ret;
        }

        public string GenericSellString()
        {

            if (GetQuadrupleBottomAndBreakdowns().Length > 0)
                return "quadrupleBottomAndBreakdowns";
            if (GetTripleBottomAndBreakdowns().Length > 0)
                return "tripleBottomAndBreakdowns";
            if (GetDescendingTripleBreakdown().Length > 0)
                return "descendingTripleBreakdown";
            if (GetDoubleBottomAndBreakdowns().Length > 0)
                return "doubleBottomAndBreakdowns";
            if (GetBearishTriangleBreakdown().Length > 0)
                return "bearishTriangleBreakdown";
            if (GetSpreadTripleBottom().Length > 0)
                return "spreadTripleBottom";
            if (GetBullishSignalReversed().Length > 0)
                return "bullishSignalReversed";
            if (GetBearishCatapultBreakdown().Length > 0)
                return "bearishCatapultBreakdown";
            if (GetBullTrap().Length > 0)
                return "bullTrap";
            if (GetPandFSell().Length > 0)
                return "sell";
            return "";
        }

        public int GenericSellCount()
        {

            int cnt = 0;
            if (GetPandFSell().Length > 0)
                cnt++;
            if (GetBullTrap().Length > 0)
                cnt++;
            if (GetBearishCatapultBreakdown().Length > 0)
                cnt++;
            if (GetBullishSignalReversed().Length > 0)
                cnt++;
            if (GetBearishTriangleBreakdown().Length > 0)
                cnt++;
            if (GetDescendingTripleBreakdown().Length > 0)
                cnt++;
            if (GetDoubleBottomAndBreakdowns().Length > 0)
                cnt++;
            if (GetTripleBottomAndBreakdowns().Length > 0)
                cnt++;
            if (GetQuadrupleBottomAndBreakdowns().Length > 0)
                cnt++;
            if (GetSpreadTripleBottom().Length > 0)
                cnt++;
            return cnt;
        }

        public double GetBoxSize()
        {

            return _boxSize;
        }
    }
}
