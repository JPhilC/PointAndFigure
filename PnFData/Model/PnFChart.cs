﻿namespace PnFData.Model
{

    public enum PnFChartSource
    {
        Share,
        Index,
        RelativeStrength
    }

    public class PnFChart: EntityData
    {
        public PnFChartSource Source { get; set; }

        public DateTime GeneratedDate { get; set; }

        public double? BoxSize { get; set; }
        public int Reversal { get; set; }

        public List<PnFColumn> Columns { get; set; }


    }
}