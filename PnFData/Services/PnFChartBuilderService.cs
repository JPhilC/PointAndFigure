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
    public abstract class PnFChartBuilderService
    {

        public bool DontResize { get; set; } = false;

        public double BoxSize { get; protected set; } = 2.0d;

        public abstract PnFChart? BuildChart(double boxSize, int reversal, DateTime uptoDate);

        public abstract bool UpdateChart(ref PnFChart chart, DateTime uptoDate);


        protected void UpdateSignals(ref PnFChart chart, int columnIndex, DateTime day)
        {
            if (chart.Columns.Count == 0 || chart.Columns.Count < columnIndex + 1)
            {
                return;
            }
            PnFSignalEnum signals = PnFSignalEnum.NotSet;
            PnFColumn currentColumn = chart.Columns[columnIndex];
            int currentIndex = currentColumn.CurrentBoxIndex;
            if (currentColumn.ShowBullishSupport)
            {
                signals |= PnFSignalEnum.AboveBullishSupport;
            }
            if (currentColumn.ColumnType == PnFColumnType.O)
            {
                // Falling
                signals |= PnFSignalEnum.IsFalling;
                // Double Bottom
                if (currentColumn.Index > 1)
                {
                    if (currentIndex < chart.Columns[columnIndex - 2].CurrentBoxIndex)
                    {
                        signals |= PnFSignalEnum.DoubleBottom;
                        // Triple Bottom
                        if (currentColumn.Index > 3)
                        {
                            if (currentIndex < chart.Columns[columnIndex - 4].CurrentBoxIndex)
                            {
                                signals |= PnFSignalEnum.TripleBottom;
                            }
                        }
                    }
                }
            }
            else
            {
                // Rising
                signals |= PnFSignalEnum.IsRising;
                // Double Top
                if (currentColumn.Index > 1)
                {
                    if (currentIndex > chart.Columns[columnIndex - 2].CurrentBoxIndex)
                    {
                        signals |= PnFSignalEnum.DoubleTop;
                        // Triple Top
                        if (currentColumn.Index > 3)
                        {
                            if (currentIndex > chart.Columns[columnIndex - 4].CurrentBoxIndex)
                            {
                                signals |= PnFSignalEnum.TripleTop;
                            }
                        }
                    }
                }
            }
            chart.Signals.Add(new PnFSignal()
            {
                PnFChart = chart,
                Day = day,
                Signals = signals,
                Value = GetValue(currentIndex)
            });
        }

        #region Helper methods ...

        /// <summary>
        /// Returns a random integer between 1000 and 5000
        /// </summary>
        /// <returns></returns>
        public static int GetRandomDelay()
        {
            Random r = new Random();
            return r.Next(1000, 5000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected double RangeBoxSize(double value)
        {
            var boxSize = value switch
            {
                < 0.025 => .025,
                < 0.05 => 0.05,
                < 0.1 => 0.1,
                < 0.2 => 0.2,
                < 0.25 => 0.25,
                < 0.5 => 0.5,
                < 1.0 => 1.0,
                < 2.0 => 2.0,
                < 3.0 => 3.0,
                < 5.00 => 5.0,
                < 10.00 => 10.0,
                < 15.0 => 15.00,
                < 20.00 => 20.00,
                < 25.00 => 25.00,
                < 50.00 => 50.00,
                < 250.00 => 250.00,
                _ => 500.00
            };

            return boxSize;
        }

        /// <summary>
        /// Compute the box size based on prices
        /// </summary>
        /// <returns></returns>
        public abstract double ComputeBoxSize();

        protected int GetIndex(double value, bool falling = false)
        {
            int index = (int)(value / BoxSize);
            if (falling && (value > (index * BoxSize)))
            {
                index++;
            }
            return index;
        }

        protected double GetValue(int index)
        {
            return index * BoxSize;
        }

        protected string GetMonthIndicator(DateTime date)
        {
            return " 123456789ABC".Substring(date.Month, 1);
        }
        #endregion
    }
}
