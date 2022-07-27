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
    public class PnFLogarithmicHiLoChartBuilderService : PnFChartBuilderService
    {

        private List<Eod> _eodList;
        private double _boxIncrement;

        public PnFLogarithmicHiLoChartBuilderService(List<Eod> eodList)
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
                Reversal = reversal,
                PriceScale = PnFChartPriceScale.Logarithmic
            };
            PnFColumn currentColumn = new PnFColumn();
            PnFColumn prevColumnOne = null;
            PnFColumn prevColumnTwo = null;
            PnFSignalEnum previousSignals = PnFSignalEnum.NotSet;
            DateTime firstDay = DateTime.MinValue;
            bool firstEod = true;
            bool firstBox = true;
            double firstHigh = 0;
            double firstHighTarget = 0;
            double firstLow = 0;
            double firstLowTarget = 0;
            int lastMonthRecorded = 0;
            int columnIndex = -1;
            double logLow;
            double logHigh;
            foreach (Eod eod in sortedList)
            {
                logLow = Math.Log(eod.Low);
                logHigh = Math.Log(eod.High);
                // System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{logHigh}\t{logLow}\t{eod.Close}");
                if (firstEod)
                {
                    //System.Diagnostics.Debug.WriteLine($"First Day {eod.Day} - {logHigh}\t{logLow}");
                    firstDay = eod.Day;
                    firstHigh = logHigh;
                    firstLow = logLow;
                    //System.Diagnostics.Debug.WriteLine($"First targets {firstHighTarget}\t{firstLowTarget}");
                    firstEod = false;
                    firstHighTarget = firstHigh + LogBoxSize;
                    firstLowTarget = firstLow - LogBoxSize;
                }
                else if (firstBox)
                {
                    // Looking to determine if start in Os or Xs'
                    if (logLow <= firstLowTarget)
                    {
                        // Start with Os (down day)
                        this.BaseValue = firstLow;
                        int newStartIndex = GetLogarithmicIndex(firstLow, true) + 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex, BullSupportIndex = newStartIndex - 1, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        columnIndex++;
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(firstLow, true), firstLow, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logLow, true), logLow, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        firstBox = false;
                        //System.Diagnostics.Debug.WriteLine($"First box O {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                    }
                    else if (logHigh >= firstHighTarget)
                    {
                        // Start with Xs (up day)
                        this.BaseValue = firstHigh;
                        int newStartIndex = GetLogarithmicIndex(firstHigh) - 1; ;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex, BullSupportIndex = newStartIndex - 1, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        columnIndex++;
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(firstHigh), firstHigh, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logHigh), logHigh, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        firstBox = false;
                        //System.Diagnostics.Debug.WriteLine($"First box X {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                    }
                    chart.BaseValue = this.BaseValue;
                    // Update signal states
                    previousSignals = UpdateSignals(ref chart, columnIndex, eod.Day, previousSignals);
                }
                else
                {
                    if (currentColumn.ColumnType == PnFColumnType.O)
                    {
                        // Chart is falling.
                        double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - 1);
                        if (logLow <= nextBox)
                        {
                            // Add the box range.
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logLow, true), logLow, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                            lastMonthRecorded = eod.Day.Month;
                            //System.Diagnostics.Debug.WriteLine($"Next O box {eod.Day} - {logHigh}\t{logLow}");
                        }
                        else
                        {
                            // Have we reversed!
                            double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + reversal);
                            if (logHigh >= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                int newBullishSupportIndex = currentColumn.BullSupportIndex + 1;
                                if (prevColumnOne != null)
                                {
                                    prevColumnTwo = prevColumnOne;
                                }
                                prevColumnOne = currentColumn;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.X,
                                    CurrentBoxIndex = newStartIndex,
                                    BullSupportIndex = newBullishSupportIndex,
                                    ShowBullishSupport = prevColumnOne.ShowBullishSupport
                                };
                                int targetIndex = GetLogarithmicIndex(logHigh);

                                // Determine if bullish support line should be shown

                                if (prevColumnTwo != null && targetIndex > prevColumnTwo.EndAtIndex && !currentColumn.ShowBullishSupport)
                                {
                                    currentColumn.ShowBullishSupport = true;
                                    // Buy signal so switch on Showing bullish support line
                                    int colIndex = columnIndex;
                                    int stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) + 1;
                                    while (colIndex > -1 && Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) < stIndex && chart.Columns[colIndex].BullSupportIndex < newBullishSupportIndex)
                                    {
                                        chart.Columns[colIndex].ShowBullishSupport = true;
                                        stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex);
                                        colIndex--;
                                    }
                                }
                                currentColumn.AddBox(PnFBoxType.X, BoxSize, targetIndex, logHigh, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                lastMonthRecorded = eod.Day.Month;
                                chart.Columns.Add(currentColumn);
                                columnIndex++;

                                //System.Diagnostics.Debug.WriteLine($"Reversed to X {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                            }
                        }
                    }
                    else
                    {
                        // Chart is rising.
                        double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + 1);
                        if (logHigh >= nextBox)
                        {
                            int targetIndex = GetLogarithmicIndex(logHigh);
                            if (prevColumnTwo != null && targetIndex > prevColumnTwo.EndAtIndex && !currentColumn.ShowBullishSupport)
                            {
                                // Buy signal so switch on Showing bullish support line
                                int colIndex = columnIndex;
                                int stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) + 1;

                                while (colIndex > -1 && Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) < stIndex && chart.Columns[colIndex].BullSupportIndex <= chart.Columns[columnIndex].BullSupportIndex)
                                {
                                    chart.Columns[colIndex].ShowBullishSupport = true;
                                    stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex);
                                    colIndex--;
                                }
                            }
                            currentColumn.AddBox(PnFBoxType.X, BoxSize, targetIndex, logHigh, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                            lastMonthRecorded = eod.Day.Month;
                            //System.Diagnostics.Debug.WriteLine($"Next X box {eod.Day} - {logHigh}\t{logLow}");
                        }
                        else
                        {
                            // Have we reversed.
                            double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - reversal);
                            if (logLow <= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                int newBullishSupportIndex = currentColumn.BullSupportIndex + 1;
                                if (prevColumnOne != null)
                                {
                                    prevColumnTwo = prevColumnOne;
                                }
                                prevColumnOne = currentColumn;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.O,
                                    CurrentBoxIndex = newStartIndex,
                                    BullSupportIndex = newBullishSupportIndex,
                                    ShowBullishSupport = prevColumnOne.ShowBullishSupport
                                };
                                currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logLow, true), logLow, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                                chart.Columns.Add(currentColumn);
                                columnIndex++;
                                //System.Diagnostics.Debug.WriteLine($"Reversed to O {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                                lastMonthRecorded = eod.Day.Month;
                            }

                        }
                    }

                    // Update signal states
                    previousSignals = UpdateSignals(ref chart, columnIndex, eod.Day, previousSignals);

                }
                currentColumn.Volume += eod.Volume;
                if (prevColumnOne != null && currentColumn.EndAt.HasValue && prevColumnOne.EndAt.HasValue && prevColumnOne.EndAt.Value.Year < currentColumn.EndAt.Value.Year)
                {
                    currentColumn.ContainsNewYear = true;
                }
                chart.GeneratedDate = eod.Day;

            }
            return chart;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="uptoDate"></param>
        /// <returns>true if succeeded without error</returns>
        public override bool UpdateChart(ref PnFChart chart, DateTime uptoDate)
        {
            bool errors = false;
            // Get the chart settings.
            double boxSize = chart.BoxSize!.Value;
            this.BoxSize = boxSize;
            this.BaseValue = chart.BaseValue!.Value;
            int reversal = chart.Reversal;
            DateTime lastUpdate = chart.GeneratedDate;
            int lastMonthRecorded = lastUpdate.Month;

            // Get the column settings.
            int columnIndex = chart.Columns.Max(c => c.Index);
            PnFColumn currentColumn = chart.Columns.FirstOrDefault(c => c.Index == columnIndex);
            PnFColumn prevColumnOne = chart.Columns.FirstOrDefault(c => c.Index == columnIndex - 1);
            PnFColumn prevColumnTwo = chart.Columns.FirstOrDefault(c => c.Index == columnIndex - 2);
            double logLow;
            double logHigh;

            if (currentColumn == null)
            {
                System.Diagnostics.Debug.WriteLine("Chart has no columns");
                return true;
            }


            List<Eod> sortedList = this._eodList.Where(s => s.Day > lastUpdate && s.Day <= uptoDate).OrderBy(s => s.Day).ToList();
            if (sortedList.Count == 0)
            {
                return true;
            }

            PnFSignalEnum previousSignals = chart.LastSignal;


            foreach (Eod eod in sortedList)
            {
                logLow = Math.Log(eod.Low);
                logHigh = Math.Log(eod.High);
                // System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{logHigh}\t{logLow}\t{eod.Close}");
                if (currentColumn.ColumnType == PnFColumnType.O)
                {
                    // Chart is falling.
                    double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - 1);
                    if (logLow <= nextBox)
                    {
                        // Add the box range.
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logLow, true), logLow, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        //System.Diagnostics.Debug.WriteLine($"Next O box {eod.Day} - {logHigh}\t{logLow}");
                    }
                    else
                    {
                        // Have we reversed!
                        double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + reversal);
                        if (logHigh >= reversalBox)
                        {
                            int newStartIndex = currentColumn.CurrentBoxIndex;
                            int newBullishSupportIndex = currentColumn.BullSupportIndex + 1;
                            if (prevColumnOne != null)
                            {
                                prevColumnTwo = prevColumnOne;
                            }
                            prevColumnOne = currentColumn;
                            currentColumn = new PnFColumn()
                            {
                                PnFChart = chart,
                                Index = currentColumn.Index + 1,
                                ColumnType = PnFColumnType.X,
                                CurrentBoxIndex = newStartIndex,
                                BullSupportIndex = newBullishSupportIndex,
                                ShowBullishSupport = prevColumnOne.ShowBullishSupport
                            };
                            // Determine if bullish support line should be shown
                            int targetIndex = GetLogarithmicIndex(logHigh);

                            if (prevColumnTwo != null && targetIndex > prevColumnTwo.EndAtIndex && !currentColumn.ShowBullishSupport)
                            {
                                currentColumn.ShowBullishSupport = true;
                                // Buy signal so switch on Showing bullish support line
                                int colIndex = columnIndex;
                                int stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) + 1;
                                while (colIndex > -1 && Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) < stIndex && chart.Columns[colIndex].BullSupportIndex < newBullishSupportIndex)
                                {
                                    chart.Columns[colIndex].ShowBullishSupport = true;
                                    stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex);
                                    colIndex--;
                                }
                            }

                            currentColumn.AddBox(PnFBoxType.X, BoxSize, targetIndex, logHigh, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                            lastMonthRecorded = eod.Day.Month;
                            chart.Columns.Add(currentColumn);
                            columnIndex++;
                            //System.Diagnostics.Debug.WriteLine($"Reversed to X {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                        }
                    }
                }
                else
                {
                    // Chart is rising.
                    double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + 1);
                    if (logHigh >= nextBox)
                    {
                        int targetIndex = GetLogarithmicIndex(logHigh);
                        if (prevColumnTwo != null && targetIndex > prevColumnTwo.EndAtIndex && !currentColumn.ShowBullishSupport)
                        {
                            // Buy signal so switch on Showing bullish support line
                            int colIndex = columnIndex;
                            int stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) + 1;

                            while (colIndex > -1 && Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex) < stIndex && chart.Columns[colIndex].BullSupportIndex <= chart.Columns[columnIndex].BullSupportIndex)
                            {
                                chart.Columns[colIndex].ShowBullishSupport = true;
                                stIndex = Math.Min(chart.Columns[colIndex].StartAtIndex, chart.Columns[colIndex].EndAtIndex);
                                colIndex--;
                            }
                        }
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, targetIndex, logHigh, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                        lastMonthRecorded = eod.Day.Month;
                        //System.Diagnostics.Debug.WriteLine($"Next X box {eod.Day} - {logHigh}\t{logLow}");
                    }
                    else
                    {
                        // Have we reversed.
                        double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - reversal);
                        if (logLow <= reversalBox)
                        {
                            int newStartIndex = currentColumn.CurrentBoxIndex;
                            int newBullishSupportIndex = currentColumn.BullSupportIndex + 1;
                            if (prevColumnOne != null)
                            {
                                prevColumnTwo = prevColumnOne;
                            }
                            prevColumnOne = currentColumn;
                            currentColumn = new PnFColumn()
                            {
                                PnFChart = chart,
                                Index = currentColumn.Index + 1,
                                ColumnType = PnFColumnType.O,
                                CurrentBoxIndex = newStartIndex,
                                BullSupportIndex = newBullishSupportIndex,
                                ShowBullishSupport = prevColumnOne.ShowBullishSupport
                            };
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logLow, true), logLow, eod.Day, (eod.Day.Month != lastMonthRecorded ? GetMonthIndicator(eod.Day) : null));
                            chart.Columns.Add(currentColumn);
                            columnIndex++;
                            //System.Diagnostics.Debug.WriteLine($"Reversed to O {eod.Day} - {logHigh}\t{logLow}, Col Index = {currentColumn.Index}");
                            lastMonthRecorded = eod.Day.Month;
                        }

                    }

                }
                currentColumn.Volume += eod.Volume;
                if (prevColumnOne != null && currentColumn.EndAt.HasValue && prevColumnOne.EndAt.HasValue && prevColumnOne.EndAt.Value.Year < currentColumn.EndAt.Value.Year)
                {
                    currentColumn.ContainsNewYear = true;
                }
                chart.GeneratedDate = eod.Day;

                // Update signal states
                previousSignals = UpdateSignals(ref chart, columnIndex, eod.Day, previousSignals);

            }
            return !errors;
        }

    }
}
