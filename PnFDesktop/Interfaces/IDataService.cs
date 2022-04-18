using PnFData.Model;
using PnFDesktop.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnFDesktop.Interfaces
{
    public interface IDataService
    {
        PnFChart GetPointAndFigureChart(string tidm, int reversal);
        PnFChart GetPointAndFigureChart(string tidm, float boxSize, int reversal);

        Task<PnFChart?> GetPointAndFigureChartAsync(Guid itemId, PnFChartSource chartSource);

        Task<IEnumerable<ShareDTO>> GetSharesAsync();

        Task<IEnumerable<IndexDTO>> GetIndicesAsync();

        Task<IEnumerable<DayDTO>> GetMarketAvailableDates(DateTime cutOff);
        Task<IEnumerable<MarketSummaryDTO>> GetMarketValuesAsync(DateTime day);

        Task<IEnumerable<ShareSummaryDTO>> GetShareValuesAsync(MarketSummaryDTO marketSummaryDTO);
    }
}
