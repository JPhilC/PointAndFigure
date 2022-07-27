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
    public class PnFLogarithmicValueChartBuilderService : PnFChartBuilderService
    {

        private List<IDayValue> _valueList;
        private double _boxIncrement;

        public PnFLogarithmicValueChartBuilderService(List<IDayValue> valueList)
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
                Reversal = reversal,
                PriceScale = PnFChartPriceScale.Logarithmic
            };


            PnFColumn currentColumn = new PnFColumn();
            PnFColumn prevColumnOne = null;

            PnFSignalEnum previousSignals = PnFSignalEnum.NotSet;
            DateTime firstDay = DateTime.MinValue;
            bool firstDayValue = true;
            bool firstBox = true;
            int lastMonthRecorded = 0;
            int columnIndex = -1;
            double firstValue = 0;
            double firstHighTarget = 0;
            double firstLowTarget = 0;
            double logValue;
            foreach (IDayValue dayValue in sortedList)
            {
                logValue = Math.Log(dayValue.Value);
                // System.Diagnostics.Debug.WriteLine($"{eod.Open}\t{eod.High}\t{eod.Low}\t{eod.Close}");
                if (firstDayValue)
                {
                    firstDay = dayValue.Day;
                    firstValue = logValue;
                    firstDayValue = false;
                    firstHighTarget = firstValue + LogBoxSize;
                    firstLowTarget = firstValue - LogBoxSize;
                    this.BaseValue = firstValue;
                    chart.BaseValue = firstValue;
                }
                else if (firstBox)
                {
                    if (logValue <= firstLowTarget)
                    {
                        // Start with Os (down day)
                        int newStartIndex = GetLogarithmicIndex(firstValue, true) + 1;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.O, CurrentBoxIndex = newStartIndex, BullSupportIndex = newStartIndex - 1, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        columnIndex++;
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(firstValue, true), firstValue, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logValue, true), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                        firstBox = false;
                    }
                    else if (logValue >= firstHighTarget)
                    {
                        // Start with Xs (up day)
                        int newStartIndex = GetLogarithmicIndex(firstValue) - 1; ;
                        currentColumn = new PnFColumn() { PnFChart = chart, Index = 0, ColumnType = PnFColumnType.X, CurrentBoxIndex = newStartIndex, BullSupportIndex = newStartIndex - 1, ContainsNewYear = true };
                        chart.Columns.Add(currentColumn);
                        columnIndex++;
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(firstValue), firstValue, firstDay, (firstDay.Month != lastMonthRecorded ? GetMonthIndicator(firstDay) : null));
                        lastMonthRecorded = firstDay.Month;
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logValue), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                        firstBox = false;
                    }
                    firstDayValue = false;
                    // Update signal states
                    previousSignals = UpdateSignals(ref chart, columnIndex, dayValue.Day, previousSignals);
                }
                else
                {
                    if (currentColumn.ColumnType == PnFColumnType.O)
                    {
                        // Chart is falling.
                        double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - 1);
                        if (logValue <= nextBox)
                        {
                            // Add the box range.
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logValue, true), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                        }
                        else
                        {
                            // Have we reversed!
                            double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + reversal);
                            if (logValue >= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                prevColumnOne = currentColumn;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.X,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logValue), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                                lastMonthRecorded = dayValue.Day.Month;
                                chart.Columns.Add(currentColumn);
                            }
                        }
                    }
                    else
                    {
                        // Chart is rising.
                        double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + 1);
                        if (logValue >= nextBox)
                        {
                            currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logValue), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                        }
                        else
                        {
                            // Have we reversed.
                            double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - reversal);
                            if (logValue <= reversalBox)
                            {
                                int newStartIndex = currentColumn.CurrentBoxIndex;
                                prevColumnOne = currentColumn;
                                currentColumn = new PnFColumn()
                                {
                                    PnFChart = chart,
                                    Index = currentColumn.Index + 1,
                                    ColumnType = PnFColumnType.O,
                                    CurrentBoxIndex = newStartIndex
                                };
                                currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logValue, true), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                                lastMonthRecorded = dayValue.Day.Month;
                                chart.Columns.Add(currentColumn);
                            }

                        }
                    }

                    // See if we had a year change in the current column.
                }

                // Update signal states
                previousSignals = UpdateSignals(ref chart, currentColumn.Index, dayValue.Day, previousSignals);

                if (prevColumnOne != null && currentColumn.EndAt.HasValue && prevColumnOne.EndAt.HasValue && prevColumnOne.EndAt.Value.Year < currentColumn.EndAt.Value.Year)
                {
                    currentColumn.ContainsNewYear = true;
                }
                chart.GeneratedDate = dayValue.Day;


            }
            return chart;
        }

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
            double logValue;

            if (currentColumn == null)
            {
                System.Diagnostics.Debug.WriteLine("Chart has no columns");
                return true;
            }


            List<IDayValue> sortedList = this._valueList.Where(s => s.Day > lastUpdate && s.Day <= uptoDate).OrderBy(s => s.Day).ToList();
            if (sortedList.Count == 0)
            {
                return true;
            }

            PnFSignalEnum previousSignals = chart.LastSignal;

            foreach (IDayValue dayValue in sortedList)
            {
                logValue = Math.Log(dayValue.Value);

                if (currentColumn.ColumnType == PnFColumnType.O)
                {
                    // Chart is falling.
                    double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - 1);
                    if (logValue <= nextBox)
                    {
                        // Add the box range.
                        currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logValue, true), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                    }
                    else
                    {
                        // Have we reversed!
                        double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + reversal);
                        if (logValue >= reversalBox)
                        {
                            int newStartIndex = currentColumn.CurrentBoxIndex;
                            prevColumnOne = currentColumn;
                            currentColumn = new PnFColumn()
                            {
                                PnFChart = chart,
                                Index = currentColumn.Index + 1,
                                ColumnType = PnFColumnType.X,
                                CurrentBoxIndex = newStartIndex
                            };
                            currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logValue), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                            chart.Columns.Add(currentColumn);
                        }
                    }
                }
                else
                {
                    // Chart is rising.
                    double nextBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex + 1);
                    if (logValue >= nextBox)
                    {
                        currentColumn.AddBox(PnFBoxType.X, BoxSize, GetLogarithmicIndex(logValue), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                        lastMonthRecorded = dayValue.Day.Month;
                    }
                    else
                    {
                        // Have we reversed.
                        double reversalBox = GetValueLogarithmic(currentColumn.CurrentBoxIndex - reversal);
                        if (logValue <= reversalBox)
                        {
                            int newStartIndex = currentColumn.CurrentBoxIndex;
                            prevColumnOne = currentColumn;
                            currentColumn = new PnFColumn()
                            {
                                PnFChart = chart,
                                Index = currentColumn.Index + 1,
                                ColumnType = PnFColumnType.O,
                                CurrentBoxIndex = newStartIndex
                            };
                            currentColumn.AddBox(PnFBoxType.O, BoxSize, GetLogarithmicIndex(logValue, true), logValue, dayValue.Day, (dayValue.Day.Month != lastMonthRecorded ? GetMonthIndicator(dayValue.Day) : null));
                            lastMonthRecorded = dayValue.Day.Month;
                            chart.Columns.Add(currentColumn);
                        }

                    }
                }


                if (prevColumnOne != null && currentColumn.EndAt.HasValue && prevColumnOne.EndAt.HasValue && prevColumnOne.EndAt.Value.Year < currentColumn.EndAt.Value.Year)
                {
                    currentColumn.ContainsNewYear = true;
                }
                chart.GeneratedDate = dayValue.Day;

                // Update signal states
                previousSignals = UpdateSignals(ref chart, currentColumn.Index, dayValue.Day, previousSignals);

            }
            return !errors;
        }
    }
}
