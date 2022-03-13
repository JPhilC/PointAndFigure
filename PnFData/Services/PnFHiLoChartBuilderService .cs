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

using System.Net.Sockets;
using PnFData.Model;

namespace PnFData.Services
{
    public class PnFHiLoChartBuilderService : PnFChartBuilderService
    {

        private List<Eod> _eodList;

        public PnFHiLoChartBuilderService(List<Eod> eodList)
        {
            this._eodList = eodList;
        }


        public override PnFChart? BuildChart(double boxSize, int reversal, DateTime uptoDate)
        {
            List<Eod> sortedList = this._eodList.Where(s => s.Day <= uptoDate).OrderBy(s => s.Day).ToList();
            if (sortedList.Count == 0)
            {
                return null;
            }

            this.BoxSize = boxSize;

            PnFChart chart = new PnFChart()
            {
                BoxSize = boxSize,
                Reversal = reversal
            };
            PnFColumn currentColumn = new PnFColumn();

            DateTime firstDay = DateTime.MinValue;
            bool firstEod = true;
            bool firstBox = true;
            double firstHigh = 0;
            double firstHighTarget = 0;
            double firstLow = 0;
            double firstLowTarget = 0;
            int lastMonthRecorded = 0;
            int lastYearRecorded = 0;
            foreach (Eod eod in sortedList)
            {
                // System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{eod.High}\t{eod.Low}\t{eod.Close}");
                if (firstEod)
                {
                    //System.Diagnostics.Debug.WriteLine($"First Day {eod.Day} - {eod.High}\t{eod.Low}");
                    firstDay = eod.Day;
                    firstHigh = eod.High;
                    firstHighTarget = (GetIndex(eod.High) + 1) * boxSize;
                    firstLow = eod.Low;
                    firstLowTarget = (GetIndex(eod.Low, true) - 1) * boxSize;
                    //System.Diagnostics.Debug.WriteLine($"First targets {firstHighTarget}\t{firstLowTarget}");
                    firstEod = false;
                }
                else if (firstBox)
                {
                    // Looking to determine if start in Os or Xs'
                    if (eod.Low <= firstLowTarget)
                    {
                        // Start with Os (down day)
                        int newStartIndex = GetIndex(firstLow, true) + 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(firstLow, true), firstLow, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low, true), eod.Low, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        firstBox = false;
                        //System.Diagnostics.Debug.WriteLine($"First box O {eod.Day} - {eod.High}\t{eod.Low}, Col Index = {currentColumn.Index}");
                    }
                    else if (eod.High >= firstHighTarget)
                    {
                        // Start with Xs (up day)
                        int newStartIndex = GetIndex(firstHigh) - 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(firstHigh), firstHigh, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(eod.High), eod.High, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        firstBox = false;
                        //System.Diagnostics.Debug.WriteLine($"First box X {eod.Day} - {eod.High}\t{eod.Low}, Col Index = {currentColumn.Index}");
                    }
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
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low, true), eod.Low, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                            lastMonthRecorded = eod.Day.Month;
                            //System.Diagnostics.Debug.WriteLine($"Next O box {eod.Day} - {eod.High}\t{eod.Low}");
                        }
                        else
                        {
                            // Have we reversed!
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex + reversal);
                            if (eod.High >= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.X,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(eod.High), eod.High, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                lastMonthRecorded = eod.Day.Month;
                                chart.Columns.Add(currentColumn);
                                //System.Diagnostics.Debug.WriteLine($"Reversed to X {eod.Day} - {eod.High}\t{eod.Low}, Col Index = {currentColumn.Index}");
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
                            lastMonthRecorded = eod.Day.Month;
                            //System.Diagnostics.Debug.WriteLine($"Next X box {eod.Day} - {eod.High}\t{eod.Low}");
                        }
                        else
                        {
                            // Have we reversed.
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex - reversal);
                            if (eod.Low <= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.O,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(eod.Low, true), eod.Low, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                chart.Columns.Add(currentColumn);
                                //System.Diagnostics.Debug.WriteLine($"Reversed to O {eod.Day} - {eod.High}\t{eod.Low}, Col Index = {currentColumn.Index}");
                                lastMonthRecorded = eod.Day.Month;
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
                lastYearRecorded = eod.Day.Year;
                chart.GeneratedDate = eod.Day;
            }
            return chart;
        }

        /// <summary>
        /// Compute the box size based on prices
        /// </summary>
        /// <returns></returns>
        public override double ComputeBoxSize()
        {
            double boxSize;
            var stats = (from d in _eodList
                         group d by 1
                into g
                         select new
                         {
                             BestLow = g.Min(l => l.Low),
                             BestHigh = g.Max(h => h.High),
                             SumHighLessLow = g.Sum(s => s.High - s.Low)
                         }).First();



            boxSize = RangeBoxSize((stats.BestLow + stats.BestHigh) * 0.5);
            //int bs = (int)(boxSize + 0.5);
            //boxSize = bs * 0.01d;
            //boxSize = stats.SumHighLessLow / _eodList.Count;
            return boxSize;
        }


    }
}
