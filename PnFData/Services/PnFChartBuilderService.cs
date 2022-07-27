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

using System.Diagnostics;
using System.Net.Sockets;
using PnFData.Model;

namespace PnFData.Services
{
    public abstract class PnFChartBuilderService
    {

        public bool DontResize { get; set; } = false;

        private double _boxSize = 2.0d;
        public double BoxSize
        {
            get => _boxSize;
            protected set
            {
                _boxSize = value;
                _logBoxSize = Math.Log(1d + (_boxSize * 0.01));
            }
        }

        private double _logBoxSize = Math.Log(1.02d);
        public double LogBoxSize
        {
            get => _logBoxSize;
        }

        public double? BaseValue { get; protected set; } = null;


        public abstract PnFChart? BuildChart(double boxSize, int reversal, DateTime uptoDate);

        public abstract bool UpdateChart(ref PnFChart chart, DateTime uptoDate);


        protected PnFSignalEnum UpdateSignals(ref PnFChart chart, int columnIndex, DateTime day, PnFSignalEnum previousSignals)
        {
            if (chart.Columns.Count == 0 || chart.Columns.Count < columnIndex + 1)
            {
                return PnFSignalEnum.NotSet;
            }
            PnFSignalEnum signals = PnFSignalEnum.NotSet;
            bool doubleSignalSet = false;
            bool tripleSignalSet = false;

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
                    if (currentIndex < chart.Columns[columnIndex - 2].EndAtIndex)
                    {
                        signals |= PnFSignalEnum.DoubleBottom;
                        // Switch off any previous tripleTop
                        previousSignals &= ~PnFSignalEnum.TripleTop;
                        doubleSignalSet = true;
                        // Triple Bottom
                        if (currentColumn.Index > 3)
                        {
                            if (currentIndex < chart.Columns[columnIndex - 4].EndAtIndex && chart.Columns[columnIndex - 2].EndAtIndex < chart.Columns[columnIndex - 4].EndAtIndex)
                            {
                                signals |= PnFSignalEnum.TripleBottom;
                                tripleSignalSet = true;
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
                    if (currentIndex > chart.Columns[columnIndex - 2].EndAtIndex)
                    {
                        signals |= PnFSignalEnum.DoubleTop;
                        // Switch off any previous tripleBottom
                        previousSignals &= ~PnFSignalEnum.TripleBottom;
                        doubleSignalSet = true;
                        // Triple Top
                        if (currentColumn.Index > 3)
                        {
                            if (currentIndex > chart.Columns[columnIndex - 4].EndAtIndex && chart.Columns[columnIndex - 2].EndAtIndex > chart.Columns[columnIndex - 4].EndAtIndex)
                            {
                                signals |= PnFSignalEnum.TripleTop;
                                tripleSignalSet = true;
                            }
                        }
                    }
                }
            }

            // Roll forward the buy/sell signal is a new signal is not set
            if (!doubleSignalSet)
            {
                if ((previousSignals & PnFSignalEnum.DoubleBottom) == PnFSignalEnum.DoubleBottom)
                {
                    signals |= PnFSignalEnum.DoubleBottom;
                }
                else if ((previousSignals & PnFSignalEnum.DoubleTop) == PnFSignalEnum.DoubleTop)
                {
                    signals |= PnFSignalEnum.DoubleTop;
                }
            }

            if (!tripleSignalSet)
            {
                if ((previousSignals & PnFSignalEnum.TripleBottom) == PnFSignalEnum.TripleBottom)
                {
                    signals |= PnFSignalEnum.TripleBottom;
                }
                else if ((previousSignals & PnFSignalEnum.TripleTop) == PnFSignalEnum.TripleTop)
                {
                    signals |= PnFSignalEnum.TripleTop;
                }
            }
            chart.Signals.Add(new PnFSignal()
            {
                PnFChart = chart,
                Day = day,
                Signals = signals,
                Value = GetValueNormal(currentIndex)
            });
            chart.LastSignal = signals;
            return signals;
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


        /*
         Traditional box sizes
                Price Range	    Box Size
                Under 0.25	    0.0625
                0.25 to 1.00	0.125
                1.00 to 5.00	0.25
                5.00 to 20.00	0.50
                20.00 to 100	1.00
                100 to 200	    2.00
                200 to 500	    4.00
                500 to 1,000	5.00
                1,000 to 25,000	50.00
                25,000 and up	500.00

         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected double RangeBoxSize(double value)
        {
            var boxSize = value switch
            {

                < 0.1 => 0.1,
                < 0.2 => 0.2,
                < 0.5 => 0.5,
                < 1.0 => 1.0,
                < 2.0 => 2.0,
                < 5.0 => 5.0,
                < 10.00 => 10.00,
                < 20.00 => 20.00,
                < 50.00 => 50.00,
                < 100.00 => 100.00,
                < 200.00 => 200.00,
                < 500.00 => 500.00,
                < 1000.00 => 1000.00,
                < 2000.00 => 2000.00,
                _ => 5000.00
            };

            return boxSize;
        }


        /// <summary>
        /// Compute the box size based on prices using the normal scale
        /// </summary>
        /// <returns></returns>
        public virtual double ComputeNormalBoxSize()
        {
            throw new NotImplementedException();
        }

        protected int GetNormalIndex(double value, bool falling = false)
        {
            int index = (int)(value / BoxSize);
            if (falling && (value > (index * BoxSize)))
            {
                index++;
            }
            return index;
        }

        /// <summary>
        /// Returns the next logarithmic based box size
        /// </summary>
        /// <param name="value">Log of the value</param>
        /// <param name="falling"></param>
        /// <returns></returns>
        protected int GetLogarithmicIndex(double value, bool falling = false)
        {
            Debug.Assert(this.BaseValue.HasValue, "BaseValue has not been set");
            int index = (int)((value - this.BaseValue.Value) / LogBoxSize);
            if (falling && (value > (index * LogBoxSize)))
            {
                index++;
            }
            return index;
        }

        protected double GetValueNormal(int index)
        {
            return index * BoxSize;
        }

        /// <summary>
        /// Return the log associated with the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected double GetValueLogarithmic(int index)
        {
            Debug.Assert(this.BaseValue.HasValue, "BaseValue has not been set");
            return this.BaseValue.Value + (index * this.LogBoxSize);
        }


        protected string GetMonthIndicator(DateTime date)
        {
            return " 123456789ABC".Substring(date.Month, 1);
        }
        #endregion
    }
}
