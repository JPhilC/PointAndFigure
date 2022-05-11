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


IF object_id('tempdb..#Signals_5','U') is not null
	DROP TABLE #Signals_5;

IF object_id('tempdb..#Signals_6','U') is not null
	DROP TABLE #Signals_6;

IF object_id('tempdb..#Signals_7','U') is not null
	DROP TABLE #Signals_7;

IF object_id('tempdb..#Signals_8','U') is not null
	DROP TABLE #Signals_8;

IF object_id('tempdb..#Signals_9','U') is not null
	DROP TABLE #Signals_9;

IF object_id('tempdb..#Signals_10','U') is not null
	DROP TABLE #Signals_10;


IF object_id('tempdb..#Pivot','U') is not null
	DROP TABLE #Pivot;

IF object_id('tempdb..#IndexResults','U') is not null
	DROP TABLE #IndexResults;

IF object_id('tempdb..#today','U') is not null
	DROP TABLE #today;

IF object_id('tempdb..#yesterday','U') is not null
	DROP TABLE #yesterday;

-- Get the latest charts for each type
-- Chart sources
--		5 - Index Bullish Percent
--		6 - Index Percent RS Buy
--		7 - Index Percent RS Rising
--		8 - Index Percent Positive Trend
--		9 - Index Percent Above 30 ema
--	   10 - Index Percent Above 10 ema

DECLARE @cutoffDate date
SET @cutoffDate = DATEADD(d, -170, GETDATE())		-- Make sure we clear the 30 week EMA period



SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_5
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 5

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_6
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 6

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_7
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 7

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_8
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 8

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_9
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 9

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #Signals_10
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 10

SELECT iv.[IndexId]
	,	iv.[Day]
	,	s5.[Signals] S5Signals
	,	s5.[Value] S5Value
	,	s6.[Signals] S6Signals
	,	s6.[Value] S6Value
	,	s7.[Signals] S7Signals
	,	s7.[Value] S7Value
	,	s8.[Signals] S8Signals
	,	s8.[Value] S8Value
	,	s9.[Signals] S9Signals
	,	s9.[Value] S9Value
	,	s10.[Signals] S10Signals
	,	s10.[Value] S10Value
INTO #pivot
FROM IndexValues iv 		
LEFT JOIN #Signals_5 s5 ON s5.IndexId = iv.[IndexId] AND s5.[Day] = iv.[Day]		
LEFT JOIN #Signals_6 s6 ON s6.IndexId = iv.[IndexId] AND s6.[Day] = iv.[Day]		
LEFT JOIN #Signals_7 s7 ON s7.IndexId = iv.[IndexId] AND s7.[Day] = iv.[Day]		
LEFT JOIN #Signals_8 s8 ON s8.IndexId = iv.[IndexId] AND s8.[Day] = iv.[Day]		
LEFT JOIN #Signals_9 s9 ON s9.IndexId = iv.[IndexId] AND s9.[Day] = iv.[Day]		
LEFT JOIN #Signals_10 s10 ON s10.IndexId = iv.[IndexId] AND s10.[Day] = iv.[Day]		

DECLARE @IsRising AS INT			= 0x0001; --       // Going up
DECLARE @IsFalling AS INT			= 0x0002; --       // Going down
DECLARE @DoubleTop AS INT			= 0x0004; --       // Double Bottom
DECLARE @DoubleBottom AS INT		= 0x0008; --       // Double Bottom
DECLARE @TripleTop AS INT			= 0x0010; --       // Triple Top
DECLARE @TripleBottom AS INT		= 0x0020; --       // Triple Bottom
DECLARE @AboveBullSupport AS INT	= 0x0040; --       // Current box is abobe bullish support level



select	[IndexId]
	,	[Day]
	, CONVERT(bit, IIF(S5Signals&@IsRising=@IsRising, 1, 0)) AS BullishPercentRising
	, CONVERT(bit, IIF(S5Signals&@DoubleTop=@DoubleTop, 1, 0)) AS BullishPercentDoubleTop
	, CONVERT(bit, IIF(S6Signals&@IsRising=@IsRising, 1, 0)) AS PercentRSBuyRising
	, CONVERT(bit, IIF(S7Signals&@IsRising=@IsRising, 1, 0)) AS PercentRsRisingRising
	, CONVERT(bit, IIF(S8Signals&@IsRising=@IsRising, 1, 0)) AS PercentPositiveTrendRising
	, CONVERT(bit, IIF(S9Signals&@IsRising=@IsRising, 1, 0)) AS PercentAbove30EmaRising
	, CONVERT(bit, IIF(S10Signals&@IsRising=@IsRising, 1, 0)) AS PercentAbove10EmaRising
	, CONVERT(bit, IIF(S5Signals&@IsFalling=@IsFalling, 1, 0)) AS BullishPercentFalling
	, CONVERT(bit, IIF(S5Signals&@DoubleBottom=@DoubleBottom, 1, 0)) AS BullishPercentDoubleBottom
	, CONVERT(bit, IIF(S6Signals&@IsFalling=@IsFalling, 1, 0)) AS PercentRSBuyFalling
	, CONVERT(bit, IIF(S7Signals&@IsFalling=@IsFalling, 1, 0)) AS PercentRsRisingFalling
	, CONVERT(bit, IIF(S8Signals&@IsFalling=@IsFalling, 1, 0)) AS PercentPositiveTrendFalling
	, CONVERT(bit, IIF(S9Signals&@IsFalling=@IsFalling, 1, 0)) AS PercentAbove30EmaFalling
	, CONVERT(bit, IIF(S10Signals&@IsFalling=@IsFalling, 1, 0)) AS PercentAbove10EmaFalling
into #IndexResults
from #pivot

UPDATE [dbo].[IndexIndicators]
	SET [BullishPercentRising] = sr.[BullishPercentRising]
	,	[BullishPercentDoubleTop] = sr.[BullishPercentDoubleTop]
	,	[PercentRSBuyRising] = sr.[PercentRSBuyRising]
	,	[PercentRsRisingRising] = sr.[PercentRsRisingRising]
	,	[PercentPositiveTrendRising] = sr.[PercentPositiveTrendRising]
	,	[PercentAbove30EmaRising] = sr.[PercentAbove30EmaRising]
	,	[PercentAbove10EmaRising] = sr.[PercentAbove10EmaRising]
	,	[BullishPercentFalling] = sr.[BullishPercentFalling]
	,	[BullishPercentDoubleBottom] = sr.[BullishPercentDoubleBottom]
	,	[PercentRSBuyFalling] = sr.[PercentRSBuyFalling]
	,	[PercentRsRisingFalling] = sr.[PercentRsRisingFalling]
	,	[PercentPositiveTrendFalling] = sr.[PercentPositiveTrendFalling]
	,	[PercentAbove30EmaFalling] = sr.[PercentAbove30EmaFalling]
	,	[PercentAbove10EmaFalling] = sr.[PercentAbove10EmaFalling]
FROM [dbo].[IndexIndicators] si
INNER JOIN #IndexResults sr ON sr.[IndexId] = si.[IndexId] AND sr.[Day] = si.[Day];


INSERT INTO [dbo].[IndexIndicators] ([Id], [IndexId], [Day] 
		, [BullishPercentRising]
		, [BullishPercentDoubleTop]
		, [PercentRSBuyRising]
		, [PercentRsRisingRising]
		, [PercentPositiveTrendRising]
		, [PercentAbove30EmaRising]
		, [PercentAbove10EmaRising]
		, [BullishPercentFalling]
		, [BullishPercentDoubleBottom]
		, [PercentRSBuyFalling]
		, [PercentRsRisingFalling]
		, [PercentPositiveTrendFalling]
		, [PercentAbove30EmaFalling]
		, [PercentAbove10EmaFalling])
	SELECT NEWID() AS Id
		, sr.[IndexId]
		, sr.[Day]
		, sr.[BullishPercentRising]
		, sr.[BullishPercentDoubleTop]
		, sr.[PercentRSBuyRising]
		, sr.[PercentRsRisingRising]
		, sr.[PercentPositiveTrendRising]
		, sr.[PercentAbove30EmaRising]
		, sr.[PercentAbove10EmaRising]
		, sr.[BullishPercentFalling]
		, sr.[BullishPercentDoubleBottom]
		, sr.[PercentRSBuyFalling]
		, sr.[PercentRsRisingFalling]
		, sr.[PercentPositiveTrendFalling]
		, sr.[PercentAbove30EmaFalling]
		, sr.[PercentAbove10EmaFalling]
	FROM #IndexResults sr
	LEFT JOIN [IndexIndicators] si ON si.[IndexId] = sr.[IndexId] AND si.[Day] = sr.[Day]
	WHERE si.[Id] IS NULL


-- Now process the new event triggers.
SELECT ii.[IndexId], ii.[Day]
		, iv.[BullishPercent]
		, ii.[BullishPercentRising]
		, ii.[BullishPercentDoubleTop]
		, ii.[BullishPercentFalling]
		, ii.[BullishPercentDoubleBottom]
		, ii.[PercentRSBuyRising]
		, ii.[PercentRsRisingRising]
		, ii.[PercentPositiveTrendRising]
		, ii.[PercentAbove30EmaRising]
		, ii.[PercentAbove10EmaRising]
		, ii.[PercentRSBuyFalling]
		, ii.[PercentRsRisingFalling]
		, ii.[PercentPositiveTrendFalling]
		, ii.[PercentAbove30EmaFalling]
		, ii.[PercentAbove10EmaFalling]
		, ROW_NUMBER() OVER(PARTITION BY ii.[IndexId] ORDER BY ii.[Day] ASC) as Ordinal#
	INTO #today
	FROM [dbo].[IndexIndicators] ii
	LEFT JOIN [dbo].[IndexValues] iv ON iv.IndexId = ii.IndexId and iv.[Day] = ii.[Day]
	-- WHERE ii.[Day] >= @cutOffDate;

DECLARE @BullAlert AS INT			= 0x0001;
DECLARE @BearAlert AS INT			= 0x0002;
DECLARE @BullConfirmed AS INT		= 0x0004;
DECLARE @BearConfirmed AS INT		= 0x0008;
DECLARE @BullConfirmedLt30 AS INT	= 0x0010;
DECLARE @BearConfirmedGt70 AS INT	= 0x0020;

UPDATE [dbo].[IndexIndicators]
		SET [NewEvents] 
		= iif(td.[BullishPercentRising]^yd.[BullishPercentRising]=1 and td.[BullishPercentRising]=1 and p.s5Value<30 and yd.[BullishPercentDoubleButtom]=1, @BullAlert, 0)
		+ iif(td.[BullishPercentFalling]^yd.[BullishPercentFalling]=1 and td.[BullishPercentFalling]=1 and p.s5Value>70 and yd.[BullishPercentDoubleTop]=1, @BearAlert, 0)
		+ iif(td.[BullishPercentDoubleTop]^yd.[BullishPercentDoubleTop]=1 and td.[BullishPercentDoubleTop]=1, @BullConfirmed, 0)	-- Bull Confirmed
		+ iif(td.[BullishPercentDoubleBottom]^yd.[BullishPercentDoubleBottom]=1 and td.[BullishPercentDoubleBottom]=1, @BearConfirmed, 0)			-- Bear Confirmed
		+ iif(td.[BullishPercentDoubleTop]^yd.[BullishPercentDoubleTop]=1 and td.[BullishPercentDoubleTop]=1 and yd.[BullishPercent]<30, @BullConfirmedLt30, 0)	-- Bull Confirmed below 30
		+ iif(td.[BullishPercentDoubleBottom]^yd.[BullishPercentDoubleBottom]=1 and td.[BullishPercentDoubleBottom]=1 and yd.[BullishPercent]>70, @BearConfirmedGt70, 0)	-- Bear Confirmed above 70
	FROM [dbo].[IndexIndicators] ii
	LEFT JOIN #today td ON td.[IndexId] = ii.[IndexId]
	LEFT JOIN #today yd ON yd.[IndexId] = td.[IndexId] and yd.[Ordinal#] = td.[Ordinal#]-1
	LEFT JOIN #Pivot p ON p.[IndexId] = yd.[IndexId] AND p.[Day]=yd.[Day]
	WHERE td.[Ordinal#] > 1
	-- WHERE ii.[Day] >= @cutoffDate AND td.[Ordinal#] > 1


RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO