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

        Task<Share> GetShareAsync(string tidm);

        Task<IEnumerable<PortfolioShareDTO>> GetPortfolioSharesAsync(Portfolio portfolio);

        Task<IEnumerable<IndexDTO>> GetIndicesAsync();

        Task<IEnumerable<DayDTO>> GetMarketAvailableDates(DateTime cutOff);
        Task<IEnumerable<MarketSummaryDTO>> GetMarketValuesAsync(DateTime day, string exchangeCode);

        Task<IEnumerable<ShareSummaryDTO>> GetShareValuesAsync(MarketSummaryDTO marketSummaryDTO);

        Task<IEnumerable<ShareSummaryDTO>> GetEventFilteredSharesAsync(ShareEvents eventFilter, DateTime day, string exchangeCode);
        
        Task<IEnumerable<Portfolio>> GetPortfoliosAsync();

        Task<Portfolio?> GetPortfolioAsync(Guid itemId);

        Task<IEnumerable<ShareSummaryDTO>> GetPortfolioValuesAsync(Portfolio portfolio, DateTime day);

        Task<IEnumerable<string>> GetExchangeCodesAsync();

        Task<bool> DeletePortfolioShareAsync(PortfolioShare portfolioShare);

        Task<bool> UpdatePortfolioAsync(Portfolio newPortfolio);

    }
}
