using PnFData.Model;

namespace PnFDesktop.Interfaces
{
    public interface IDataService
    {
        PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal);
    }
}
