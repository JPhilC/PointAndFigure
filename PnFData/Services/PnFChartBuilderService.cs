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



#region Helper methods ...


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected double RangeBoxSize(double value)
        {
            var boxSize = value switch
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
