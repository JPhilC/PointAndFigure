using PnFData.Model;
using PnFDesktop.Interfaces;
using System;

namespace PnFDesktop.Services
{
    public class DataService : IDataService
    {
        public PnFChart GetPointAndFigureChart(string tidm, int reversal)
        {
            return ChartBuilderService.GenerateHiLoChart(tidm, reversal, DateTime.Now.Date);
        }

        public PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal)
        {
            throw new NotImplementedException();
        }
    }
}
