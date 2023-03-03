/****** Script for SelectTopNRows command from SSMS  ******/
SELECT s.[Tidm]
	  ,s.[Name]
	  ,s.MarketCapMillions
      ,si.[ClosedAboveEma10]
      ,si.[AboveBullSupport]
      ,si.[NewEvents]
      ,si.[Ema1]
      ,si.[Ema5]
  FROM [PnFData].[dbo].[ShareIndicators] si
  LEFT JOIN [PnFData].[dbo].[Shares] s ON s.[Id] = si.[ShareId]
  LEFT JOIN [PnFData].[dbo].[EodPrices] p ON p.[ShareId] = si.[ShareId] AND p.[Day] = si.[Day]
  WHERE si.[Day] = '2022-10-07'
		AND si.[Rising] = 1 
		AND si.[DoubleBottom] = 1
		AND s.[ExchangeSubCode] = 'AIM'
		AND p.[AdjustedClose] > si.[Ema5]
  ORDER BY Tidm


  -- Rising in a Sell signal
  SELECT CONVERT(DATE ,si.[Day]) AS [Date] 
		,'' AS [Time]
		, REPLACE(s.[Tidm], '.LON', '') AS [Ticker]
		,'' AS [Type]
		,'' AS [Shares]
		,'' AS [Price]
		,'' AS [Broker]
		,'' AS [Stamp]
		,'' AS [Total]
		,'' AS [NonCash]
		,'' AS [Note]
  FROM [PnFData].[dbo].[ShareIndicators] si
  LEFT JOIN [PnFData].[dbo].[Shares] s ON s.[Id] = si.[ShareId]
  LEFT JOIN [PnFData].[dbo].[EodPrices] p ON p.[ShareId] = si.[ShareId] AND p.[Day] = si.[Day]
  WHERE si.[Day] = '2022-10-07'
		AND si.[Rising] = 1 
		AND si.[DoubleBottom] = 1
		AND s.[ExchangeSubCode] = 'AIM'
		AND p.[AdjustedClose] > si.[Ema5]
  ORDER BY Tidm

  -- Falling in a Buy signal
  SELECT CONVERT(DATE ,si.[Day]) AS [Date] 
		,'' AS [Time]
		, REPLACE(s.[Tidm], '.LON', '') AS [Ticker]
		,'' AS [Type]
		,'' AS [Shares]
		,'' AS [Price]
		,'' AS [Broker]
		,'' AS [Stamp]
		,'' AS [Total]
		,'' AS [NonCash]
		,'' AS [Note]
  FROM [PnFData].[dbo].[ShareIndicators] si
  LEFT JOIN [PnFData].[dbo].[Shares] s ON s.[Id] = si.[ShareId]
  LEFT JOIN [PnFData].[dbo].[EodPrices] p ON p.[ShareId] = si.[ShareId] AND p.[Day] = si.[Day]
  WHERE si.[Day] = '2022-10-07'
		AND si.[Falling] = 1 
		AND si.[DoubleTop] = 1
		AND s.[ExchangeSubCode] = 'AIM'
		AND p.[AdjustedClose] < si.[Ema5]
  ORDER BY Tidm
