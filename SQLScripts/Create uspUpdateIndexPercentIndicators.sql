USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspUpdateIndexPercentIndicators]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspUpdateIndexPercentIndicators] 
	AS
SET NOCOUNT ON	
RAISERROR (N'Updating index indicators ...', 0, 0) WITH NOWAIT;

	IF object_id('tempdb..#LatestCharts','U') is not null
	DROP TABLE #LatestCharts;

IF object_id('tempdb..#SortedByIndex','U') is not null
	DROP TABLE #SortedByIndex;

IF object_id('tempdb..#rising','U') is not null
	DROP TABLE #rising;


IF object_id('tempdb..#falling','U') is not null
	DROP TABLE #falling;


IF object_id('tempdb..#IndexResults','U') is not null
	DROP TABLE #IndexResults;

-- Get the latest day from EodPrices
DECLARE @Day Datetime2(7)
SELECT @Day = MAX([Day])
	FROM [EodPrices]

-- Get the latest charts for each type
-- Chart sources
--		5 - Index Bullish Percent
--		6 - Index Percent RS Buy
--		7 - Index Percent RS Rising
--		8 - Index Percent Positive Trend
--		9 - Index Percent Above 30 ema
--	   10 - Index Percent Above 10 ema
SELECT sc.[IndexId]
	, c.[Source]
	, ROW_NUMBER() OVER(PARTITION BY sc.[IndexId], c.[Source] ORDER BY c.GeneratedDate DESC) as Ordinal# 
	, sc.[ChartId]
	, c.[GeneratedDate]
	, c.[CreatedAt]
	INTO #LatestCharts
	FROM IndexCharts sc
	LEFT JOIN PnFCharts c on c.Id = sc.ChartId
	WHERE c.[Source] IN (5, 6, 7, 8, 9, 10)


SELECT col.[Id]
      ,col.[PnFChartId]
      ,col.[ColumnType]
      ,col.[CurrentBoxIndex]
      ,col.[Index]
	  ,ROW_NUMBER() OVER(PARTITION BY [PnfChartId] ORDER BY [Index] DESC) AS Ordinal#
	  ,c.[Source]
	  ,sc.[IndexId]
  INTO #SortedByIndex
  FROM [PnFColumns] col
  LEFT JOIN [PnFCharts] c ON c.Id = col.PnFChartId
  LEFT JOIN [IndexCharts] sc ON sc.ChartId = col.PnFChartId
  WHERE col.[PnFChartId] IN (SELECT ChartId FROM #LatestCharts WHERE Ordinal# = 1) 
  ORDER BY [PnfChartId], [Index] DESC

-- The Bulls
SELECT sbi.[PnfChartId], sbi.[IndexId], sbi.[Source], [CurrentBoxIndex] AS H1
	INTO #rising
	FROM #SortedByIndex sbi
	WHERE sbi.[Ordinal#] = 1 AND sbi.[ColumnType]=1;

-- The Bears
SELECT sbi.[PnfChartId], sbi.[IndexId], sbi.[Source], [CurrentBoxIndex] AS L1
	INTO #falling
	FROM #SortedByIndex sbi
	WHERE sbi.[Ordinal#] = 1 AND sbi.[ColumnType]=0;

select s.[Id] as [IndexId]
	, s.[ExchangeCode]
	, s.[ExchangeSubCode]
	, s.[SuperSector]
	, @Day as [Day]
	, CONVERT(bit, IIF(r5.[IndexId] IS NOT NULL, 1, 0)) AS BullishPercentRising
	, CONVERT(bit, IIF(r6.[IndexId] IS NOT NULL, 1, 0)) AS PercentRSBuyRising
	, CONVERT(bit, IIF(r7.[IndexId] IS NOT NULL, 1, 0)) AS PercentRsRisingRising
	, CONVERT(bit, IIF(r8.[IndexId] IS NOT NULL, 1, 0)) AS PercentPositiveTrendRising
	, CONVERT(bit, IIF(r9.[IndexId] IS NOT NULL, 1, 0)) AS PercentAbove30EmaRising
	, CONVERT(bit, IIF(r10.[IndexId] IS NOT NULL, 1, 0)) AS PercentAbove10EmaRising
	, CONVERT(bit, IIF(f5.[IndexId] IS NOT NULL, 1, 0)) AS BullishPercentFalling
	, CONVERT(bit, IIF(f6.[IndexId] IS NOT NULL, 1, 0)) AS PercentRSBuyFalling
	, CONVERT(bit, IIF(f7.[IndexId] IS NOT NULL, 1, 0)) AS PercentRsRisingFalling
	, CONVERT(bit, IIF(f8.[IndexId] IS NOT NULL, 1, 0)) AS PercentPositiveTrendFalling
	, CONVERT(bit, IIF(f9.[IndexId] IS NOT NULL, 1, 0)) AS PercentAbove30EmaFalling
	, CONVERT(bit, IIF(f10.[IndexId] IS NOT NULL, 1, 0)) AS PercentAbove10EmaFalling
into #IndexResults
from [Indices] s
left join #rising r5 ON r5.[IndexId] = s.Id AND r5.[Source] = 5
left join #rising r6 ON r6.[IndexId] = s.Id AND r6.[Source] = 6
left join #rising r7 ON r7.[IndexId] = s.Id AND r7.[Source] = 7
left join #rising r8 ON r8.[IndexId] = s.Id AND r8.[Source] = 8
left join #rising r9 ON r9.[IndexId] = s.Id AND r9.[Source] = 9
left join #rising r10 ON r10.[IndexId] = s.Id AND r10.[Source] = 10
left join #falling f5 ON f5.[IndexId] = s.Id AND f5.[Source] = 5
left join #falling f6 ON f6.[IndexId] = s.Id AND f6.[Source] = 6
left join #falling f7 ON f7.[IndexId] = s.Id AND f7.[Source] = 7
left join #falling f8 ON f8.[IndexId] = s.Id AND f8.[Source] = 8
left join #falling f9 ON f9.[IndexId] = s.Id AND f9.[Source] = 9
left join #falling f10 ON f10.[IndexId] = s.Id AND f10.[Source] = 10
order by [SuperSector], [ExchangeSubCode]

UPDATE si
	SET si.[BullishPercentRising] = sr.[BullishPercentRising]
	,	si.[PercentRSBuyRising] = sr.[PercentRSBuyRising]
	,	si.[PercentRsRisingRising] = sr.[PercentRsRisingRising]
	,	si.[PercentPositiveTrendRising] = sr.[PercentPositiveTrendRising]
	,	si.[PercentAbove30EmaRising] = sr.[PercentAbove30EmaRising]
	,	si.[PercentAbove10EmaRising] = sr.[PercentAbove10EmaRising]
	,	si.[BullishPercentFalling] = sr.[BullishPercentFalling]
	,	si.[PercentRSBuyFalling] = sr.[PercentRSBuyFalling]
	,	si.[PercentRsRisingFalling] = sr.[PercentRsRisingFalling]
	,	si.[PercentPositiveTrendFalling] = sr.[PercentPositiveTrendFalling]
	,	si.[PercentAbove30EmaFalling] = sr.[PercentAbove30EmaFalling]
	,	si.[PercentAbove10EmaFalling] = sr.[PercentAbove10EmaFalling]
FROM [dbo].[IndexIndicators] si
INNER JOIN #IndexResults sr
ON sr.[IndexId] = si.[IndexId]
	AND sr.[Day] = si.[Day];


INSERT INTO [dbo].[IndexIndicators] ([Id], [IndexId], [Day] 
		, [BullishPercentRising]
		, [PercentRSBuyRising]
		, [PercentRsRisingRising]
		, [PercentPositiveTrendRising]
		, [PercentAbove30EmaRising]
		, [PercentAbove10EmaRising]
		, [BullishPercentFalling]
		, [PercentRSBuyFalling]
		, [PercentRsRisingFalling]
		, [PercentPositiveTrendFalling]
		, [PercentAbove30EmaFalling]
		, [PercentAbove10EmaFalling])
	SELECT NEWID() AS Id
		, sr.[IndexId]
		, sr.[Day]
		, sr.[BullishPercentRising]
		, sr.[PercentRSBuyRising]
		, sr.[PercentRsRisingRising]
		, sr.[PercentPositiveTrendRising]
		, sr.[PercentAbove30EmaRising]
		, sr.[PercentAbove10EmaRising]
		, sr.[BullishPercentFalling]
		, sr.[PercentRSBuyFalling]
		, sr.[PercentRsRisingFalling]
		, sr.[PercentPositiveTrendFalling]
		, sr.[PercentAbove30EmaFalling]
		, sr.[PercentAbove10EmaFalling]
	FROM #IndexResults sr
	LEFT JOIN [IndexIndicators] si ON si.[IndexId] = sr.[IndexId] AND si.[Day] = sr.[Day]
	WHERE si.[Id] IS NULL

RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO