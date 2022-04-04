USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspUpdateIndexIndicators]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspUpdateIndexIndicators] 
	AS
SET NOCOUNT ON	
RAISERROR (N'Updating index indicators ...', 0, 0) WITH NOWAIT;

	IF object_id('tempdb..#LatestCharts','U') is not null
	DROP TABLE #LatestCharts;

IF object_id('tempdb..#SortedByIndex','U') is not null
	DROP TABLE #SortedByIndex;

IF object_id('tempdb..#rising','U') is not null
	DROP TABLE #rising;

IF object_id('tempdb..#doubleTop','U') is not null
	DROP TABLE #doubleTop;

	IF object_id('tempdb..#falling','U') is not null
	DROP TABLE #falling;

IF object_id('tempdb..#doubleBottom','U') is not null
	DROP TABLE #doubleBottom;

IF object_id('tempdb..#IndexResults','U') is not null
	DROP TABLE #IndexResults;

-- Get the latest day from EodPrices
DECLARE @Day Datetime2(7)
SELECT @Day = MAX([Day])
	FROM [EodPrices]

-- Get the latest charts for each type
-- Chart sources
--		1 - Index
--		4 - Index (Sector) RS
SELECT sc.[IndexId]
	, c.[Source]
	, ROW_NUMBER() OVER(PARTITION BY sc.[IndexId], c.[Source] ORDER BY c.GeneratedDate DESC) as Ordinal# 
	, sc.[ChartId]
	, c.[GeneratedDate]
	, c.[CreatedAt]
	INTO #LatestCharts
	FROM IndexCharts sc
	LEFT JOIN PnFCharts c on c.Id = sc.ChartId
	WHERE c.[Source] IN (1, 4)


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

SELECT sbi.[PnfChartId], sbi.[IndexId], sbi.[Source], sbi.[CurrentBoxIndex] AS H3, c1.H1  
	INTO #doubleTop
	FROM #SortedByIndex AS sbi
	JOIN #rising AS c1 ON c1.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c1.H1;

-- The Bears
SELECT sbi.[PnfChartId], sbi.[IndexId], sbi.[Source], [CurrentBoxIndex] AS L1
	INTO #falling
	FROM #SortedByIndex sbi
	WHERE sbi.[Ordinal#] = 1 AND sbi.[ColumnType]=0;

SELECT sbi.[PnfChartId], sbi.[IndexId], sbi.[Source], sbi.[CurrentBoxIndex] AS L3, f.L1  
	INTO #doubleBottom
	FROM #SortedByIndex AS sbi
	JOIN #falling AS f ON f.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > f.L1;

select s.[Id] as [IndexId]
	, s.[ExchangeCode]
	, s.[ExchangeSubCode]
	, s.[SuperSector]
	, @Day as [Day]
	, CONVERT(bit, IIF(sh.[IndexId] IS NOT NULL, 1, 0)) AS Rising
	, CONVERT(bit, IIF(shdt.[IndexId] IS NOT NULL, 1, 0)) AS Buy
	, CONVERT(bit, IIF(srs.[IndexId] IS NOT NULL, 1, 0)) AS RsRising
	, CONVERT(bit, IIF(srsBuy.[IndexId] IS NOT NULL, 1, 0)) AS RsBuy
	, CONVERT(bit, IIF(f.[IndexId] IS NOT NULL, 1, 0)) AS Falling
	, CONVERT(bit, IIF(fdb.[IndexId] IS NOT NULL, 1, 0)) AS Sell
	, CONVERT(bit, IIF(srsf.[IndexId] IS NOT NULL, 1, 0)) AS RsFalling
	, CONVERT(bit, IIF(srsSell.[IndexId] IS NOT NULL, 1, 0)) AS RsSell
into #IndexResults
from [Indices] s
left join #rising sh ON sh.[IndexId] = s.Id AND sh.[Source] = 1
left join #doubleTop shdt ON shdt.[IndexId] = s.Id AND shdt.[Source] = 1
left join #rising srs ON srs.[IndexId] = s.Id AND srs.[Source] = 4
left join #doubleTop srsBuy ON srsBuy.[IndexId] = s.Id AND srsBuy.[Source] = 4
left join #falling f ON f.[IndexId] = s.Id AND f.[Source] = 1
left join #doubleBottom fdb ON fdb.[IndexId] = s.Id AND fdb.[Source] = 1
left join #falling srsf ON srsf.[IndexId] = s.Id AND srsf.[Source] = 4
left join #doubleBottom srsSell ON srsSell.[IndexId] = s.Id AND srsSell.[Source] = 4
order by [SuperSector], [ExchangeSubCode]

UPDATE si
	SET si.[Rising] = sr.[Rising]
	,	si.[Buy] = sr.[Buy]
	,	si.[RsRising] = sr.[RsRising]
	,	si.[RsBuy] = sr.[RsBuy]
	,	si.Falling = sr.Falling
	,	si.[Sell] = sr.[Sell]
	,	si.[RsFalling] = sr.[RsFalling]
	,	si.[RsSell] = sr.[RsSell]
FROM [dbo].[IndexIndicators] si
INNER JOIN #IndexResults sr
ON sr.[IndexId] = si.[IndexId]
	AND sr.[Day] = si.[Day];


INSERT INTO [dbo].[IndexIndicators] ([Id], [IndexId], [Day], [Rising], [Buy], [RsRising],[RsBuy]
		, [Falling], [Sell], [RsFalling], [RsSell])
	SELECT NEWID() AS Id
		, sr.[IndexId]
		, sr.[Day]
		, sr.[Rising]
		, sr.[Buy]
		, sr.[RsRising]
		, sr.[RsBuy]
		, sr.[Falling]
		, sr.[Sell]
		, sr.[RsFalling]
		, sr.[RsSell]
	FROM #IndexResults sr
	LEFT JOIN [IndexIndicators] si ON si.[IndexId] = sr.[IndexId] AND si.[Day] = sr.[Day]
	WHERE si.[Id] IS NULL

RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO