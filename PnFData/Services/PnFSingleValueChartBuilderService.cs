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
using PnFData.Interfaces;
using PnFData.Model;

namespace PnFData.Services
{
    public class PnFSingleValueChartBuilderService : PnFChartBuilderService
    {

        private List<IDayValue> _valueList;

        public PnFSingleValueChartBuilderService(List<IDayValue> valueList)
        {
            this._valueList = valueList;
        }


        public override PnFChart? BuildChart(double boxSize, int reversal, DateTime uptoDate)
        {
            List<IDayValue> sortedList = this._valueList.Where(s => s.Day <= uptoDate).OrderBy(s => s.Day).ToList();
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

            bool firstDayValue = true;
            int lastMonthRecorded = 0;
            int lastYearRecorded = 0;
            foreach (IDayValue dayValue in sortedList)
            {
                // System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{eod.High}\t{eod.Low}\t{eod.Close}");
                if (firstDayValue)
                {
                    IDayValue nextValue = sortedList[1];
                    if (nextValue.Value < dayValue.Value)
                    {
                        // Start with Os (down day)
                        int newStartIndex = GetIndex(dayValue.Value) + 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex, ContainsNewYear = true };
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(dayValue.Value, true), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                        chart.Columns.Add(currentColumn);
                    }
                    else
                    {
                        // Start with Xs (up day)
                        int newStartIndex = GetIndex(dayValue.Value) - 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex, ContainsNewYear = true };
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(dayValue.Value), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                        chart.Columns.Add(currentColumn);
                    }
                    firstDayValue = false;
                }
                else
                {
                    if (currentColumn.ColumnType == PnFColumnType.O)
                    {
                        // Chart is falling.
                        double nextBox = GetValue(currentColumn.CurrentBoxIndex - 1);
                        if (dayValue.Value <= nextBox)
                        {
                            // Add the box range.
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(dayValue.Value, true), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                        }
                        else
                        {
                            // Have we reversed!
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex + reversal);
                            if (dayValue.Value >= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.X,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(dayValue.Value), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                                lastMonthRecorded = dayValue.Day.Month;
                                chart.Columns.Add(currentColumn);
                            }
                        }
                    }
                    else
                    {
                        // Chart is rising.
                        double nextBox = GetValue(currentColumn.CurrentBoxIndex + 1);
                        if (dayValue.Value >= nextBox)
                        {
                            currentColumn.AddBox(PnFBoxType.X, BoxSize, GetIndex(dayValue.Value), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                        }
                        else
                        {
                            // Have we reversed.
                            double reversalBox = GetValue(currentColumn.CurrentBoxIndex - reversal);
                            if (dayValue.Value <= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.O,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.O, BoxSize, GetIndex(dayValue.Value, true), dayValue.Value, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                                lastMonthRecorded = dayValue.Day.Month;
                                chart.Columns.Add(currentColumn);
                            }

                        }
                    }

                    // See if we had a year change in the current column.
                    if (dayValue.Day.Year != lastYearRecorded)
                    {
                        currentColumn.ContainsNewYear = true;
                    }
                }
                lastYearRecorded = dayValue.Day.Year;
                chart.GeneratedDate = dayValue.Day;
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
            var stats = (from d in _valueList
                         group d by 1
                into g
                         select new
                         {
                             BestLow = g.Min(l => l.Value),
                             BestHigh = g.Max(h => h.Value)
                         }).First();


            boxSize = RangeBoxSize((stats.BestLow + stats.BestHigh) * 0.5);
            //int bs = (int)(boxSize + 0.5);
            //boxSize = bs * 0.01d;
            //boxSize = stats.SumHighLessLow / _eodList.Count;
            return boxSize;
        }

    }
}
