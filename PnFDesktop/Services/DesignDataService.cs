using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFData.Model;
using PnFDesktop.Controls;
using PnFDesktop.Interfaces;

namespace PnFDesktop.Services
{
    public class DesignDataService : IDataService
    {
        public PnFChart GetPointAndFigureChart(string tidm, int reversal)
        {
            throw new NotImplementedException();
        }

        public PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal)
        {
            PnFChart chart = new PnFChart()
            {
                Id = Guid.NewGuid(),
                Source = PnFChartSource.Share,
                GeneratedDate = DateTime.Now.Date,
                BoxSize = 5d,
                Reversal = 3,
            };
            chart.Columns.Add(
                new PnFColumn()
                {
                    Id = Guid.NewGuid(),
                    PnFChartId = chart.Id,
                    ColumnType = PnFColumnType.X,
                    Index = 0,
                    CurrentBoxIndex = 9,
                    PnFChart = chart
                });
            chart.Columns.Add(
                new PnFColumn()
                {
                    Id = Guid.NewGuid(),
                    PnFChartId = chart.Id,
                    ColumnType = PnFColumnType.O,
                    CurrentBoxIndex = 16,
                    Index = 1,
                    PnFChart = chart
                });
            chart.Columns.Add(
                new PnFColumn()
                {
                    Id = Guid.NewGuid(),
                    PnFChartId = chart.Id,
                    ColumnType = PnFColumnType.X,
                    Index = 2,
                    CurrentBoxIndex = 7,
                    PnFChart = chart
                });

            chart.Columns[0].AddBox(PnFBoxType.X, 5, 10, 51, new DateTime(2022, 01, 01));
            chart.Columns[0].AddBox(PnFBoxType.X, 5, 11, 56, new DateTime(2022, 01, 15));
            chart.Columns[0].AddBox(PnFBoxType.X, 5, 14, 72, new DateTime(2022, 01, 28));
            chart.Columns[0].AddBox(PnFBoxType.X, 5, 15, 75, new DateTime(2022, 02, 02), "2");
            chart.Columns[0].AddBox(PnFBoxType.X, 5, 16, 81, new DateTime(2022, 02, 05));

            chart.Columns[1].AddBox(PnFBoxType.O, 5, 14, 69, new DateTime(2022, 02, 16));
            chart.Columns[1].AddBox(PnFBoxType.O, 5, 13, 64, new DateTime(2022, 02, 17));
            chart.Columns[1].AddBox(PnFBoxType.O, 5, 9, 44, new DateTime(2022, 03, 03), "3");
            chart.Columns[1].AddBox(PnFBoxType.O, 5, 8, 39, new DateTime(2022, 03, 15));
            chart.Columns[1].AddBox(PnFBoxType.O, 5, 7, 34, new DateTime(2022, 03, 20));

            chart.Columns[2].AddBox(PnFBoxType.X, 5, 9, 46, new DateTime(2022, 03, 23));
            chart.Columns[2].AddBox(PnFBoxType.X, 5, 10, 51, new DateTime(2022, 04, 3), "4");
            chart.Columns[2].AddBox(PnFBoxType.X, 5, 12, 61, new DateTime(2022, 04, 23));
            chart.Columns[2].AddBox(PnFBoxType.X, 5, 18, 91, new DateTime(2022, 05, 01), "5");

            return chart;
        }
    }
}
