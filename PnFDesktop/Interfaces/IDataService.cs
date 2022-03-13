using PnFData.Model;
using System;
using System.Threading.Tasks;

namespace PnFDesktop.Interfaces
{
    public interface IDataService
    {
        PnFChart GetPointAndFigureChart(string tidm, int reversal);
        PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal);

        Task<PnFChart?> GetPointAndFigureChartAsync(Guid itemId, PnFChartSource chartSource);
    }
}
