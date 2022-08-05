USE [PnFData]
GO

SELECT [Day]
      ,[BullishPercentFalling] * -1 AS [BullishPercentFalling]
      ,[BullishPercentRising]
      ,[PercentAbove10EmaFalling] * -1 AS [PercentAbove10EmaFalling]
      ,[PercentAbove10EmaRising]
      ,[PercentAbove30EmaFalling] * -1 AS [PercentAbove30EmaFalling]
      ,[PercentAbove30EmaRising]
      ,[PercentPositiveTrendFalling] * -1 AS [PercentPositiveTrendFalling]
      ,[PercentPositiveTrendRising]
      ,[PercentRSBuyFalling] * -1 AS [PercentRSBuyFalling]
      ,[PercentRSBuyRising]
      ,[PercentRsRisingFalling] * -1 AS [PercentRsRisingFalling]
      ,[PercentRsRisingRising]
      ,[BullishPercentDoubleBottom] * -1 AS [BullishPercentDoubleBottom]
      ,[BullishPercentDoubleTop]
      ,[HighLowIndexRising]
      ,[HighLowIndexFalling] * -1 AS [HighLowIndexFalling]
      ,[HighLowIndexBuy]
      ,[HighLowIndexSell] * -1 AS [HighLowIndexSell]
      ,[PercentAbove10EmaBuy]
      ,[PercentAbove10EmaSell] * -1 AS [PercentAbove10EmaSell]
      ,[PercentAbove30EmaBuy]
      ,[PercentAbove30EmaSell] * -1 AS [PercentAbove30EmaSell]
      ,[PercentPositiveTrendBuy]
      ,[PercentPositiveTrendSell] * -1 AS [PercentPositiveTrendSell]
      ,[PercentRSBuyBuy]
      ,[PercentRSBuySell] * -1 AS [PercentRSBuySell]
      ,[PercentRsRisingBuy]
      ,[PercentRsRisingSell] * -1 AS [PercentRsRisingSell]
  FROM [dbo].[IndexIndicators]
  WHERE IndexId='E2175119-C2E5-42D4-8216-AA10E2A55614' AND [Day] >= '2020-01-01'
  ORDER BY [Day] ASC

GO


