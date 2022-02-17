/*
 * Converted from original java code by Joseph McVerry.
 * Copyright (c) 2010-21, Joseph McVerry  Raleigh NC 
 *
*/
/* Copyright(C) 2022  Phil Crompton (phil@unitysoftware.co.uk)
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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


        public PnFChart BuildHighLowChart(double boxSize, int reversal)
        {
            PnFChart chart = new PnFChart()
            {
                BoxSize = boxSize,
                Reversal = reversal
            };
            List<Eod> sortedList = this._eodList.OrderBy(s => s.Day).ToList();
            bool firstEod = true;
            int lastMonthRecorded = 0;
            int lastYearRecorded = 0;
            PnFColumn currentColumn = new PnFColumn();
            foreach (Eod eod in sortedList)
            {
                System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{eod.High}\t{eod.Low}\t{eod.Close}");
                if (firstEod)
                {
                    if (eod.Open > eod.Close)
                    {
                        // Start with Os (down day)
                        int newStartIndex = GetIndex(eod.Low)+1;
                        currentColumn = new PnFColumn() { ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex, ContainsNewYear = true};
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low), eod.Low, eod.Day);
                        chart.Columns.Add(currentColumn);
                    }
                    else
                    {
                        // Start with Xs (up day)
                        int newStartIndex = GetIndex(eod.High)-1;
                        currentColumn = new PnFColumn() { ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex, ContainsNewYear = true};
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(eod.High), eod.High, eod.Day);
                    }
                    firstEod = false;
                }
                else
                {
                    if (currentColumn.ColumnType == PnFColumnType.O)
                    {
                        // Chart is falling.
                        double nextBox = GetValue(currentColumn.CurrentBoxIndex - 1);
                        if (eod.Low <= nextBox)
                        {
                            // Add the box range.
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low), eod.Low, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day): null));
                        }
                        else
                        {
                            // Have we reversed!
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex + reversal);
                            if (eod.High >= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn() { ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex};
                                currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(eod.High), eod.High, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                chart.Columns.Add(currentColumn);
                            }
                        }
                    }
                    else
                    {
                        // Chart is rising.
                        double nextBox = GetValue(currentColumn.CurrentBoxIndex + 1);
                        if (eod.High >= nextBox)
                        {
                            currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(eod.High), eod.High, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        }
                        else
                        {
                            // Have we reversed.
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex - reversal);
                            if (eod.Low <= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn() { ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex};
                                currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low), eod.Low, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                chart.Columns.Add(currentColumn);
                            }

                        }
                    }

                    // See if we had a year change in the current column.
                    if (eod.Day.Year != lastYearRecorded)
                    {
                        currentColumn.ContainsNewYear = true;
                    }
                }
                currentColumn.Volume += eod.Volume;
                lastMonthRecorded = eod.Day.Month;
                lastYearRecorded = eod.Day.Year;

            }
            return chart;
        }




#region Helper methods ...

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

        /// <summary>
        /// Compute the box size based on prices
        /// </summary>
        /// <returns></returns>
        public double ComputeBoxSize()
        {
            double boxSize;
            var stats = (from d in _eodList
                group d by 1
                into g
                select new
                {
                    BestLow = g.Min(l => l.Low),
                    BestHign = g.Max(h => h.High),
                    SumHighLessLow = g.Sum(s => s.High - s.Low)
                }).First();



            boxSize = SetBoxSize((stats.BestLow + stats.BestHign) * 0.5);
            //int bs = (int)(boxSize + 0.5);
            //boxSize = bs * 0.01d;
            //boxSize = stats.SumHighLessLow / _eodList.Count;
            return boxSize;
        }

        private int GetIndex(double value)
        {
            return (int)(value / BoxSize);
        }

        private double GetValue(int index)
        {
            return index * BoxSize;
        }

        private string GetMonthIndicator(DateTime date)
        {
            return " 123456789ABC".Substring(date.Month, 1);
        }
        #endregion
    }
}
