USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspUpdateShareIndicators]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspUpdateShareIndicators] 
	AS

RAISERROR (N'Updating share indicators ...', 0, 0) WITH NOWAIT;

SET NOCOUNT ON

IF object_id('tempdb..#PriceSignals','U') is not null
	DROP TABLE #PriceSignals;

IF object_id('tempdb..#RsSignals','U') is not null
DROP TABLE #RsSignals;

IF object_id('tempdb..#PeerRsSignals','U') is not null
	DROP TABLE #PeerRsSignals;

IF object_id('tempdb..#Pivot','U') is not null
	DROP TABLE #Pivot;

IF object_id('tempdb..#ShareResults','U') is not null
	DROP TABLE #ShareResults;

IF object_id('tempdb..#today','U') is not null
	DROP TABLE #today;

DECLARE @cutoffDate date
SET @cutoffDate = DATEADD(d, -170, GETDATE())		-- 170 days should clear 30 week EMA
--SET @cutoffDate = CONVERT(DATETIME, '2018-01-01')	-- Process all


SELECT sc.[ShareId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #PriceSignals
	FROM [PnFCharts] c
	JOIN [ShareCharts] sc ON sc.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 0 and s.[Day] >= @cutoffDate

SELECT sc.[ShareId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #RsSignals
	FROM [PnFCharts] c
	JOIN [ShareCharts] sc ON sc.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 2 and s.[Day] >= @cutoffDate

SELECT sc.[ShareId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #PeerRsSignals
	FROM [PnFCharts] c
	INNER JOIN [ShareCharts] sc ON sc.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 3 and s.[Day] >= @cutoffDate


SELECT q.[ShareId]
	,	q.[Day]
	,	vs.[Signals] PriceSignals
	,	vs.[Value] PriceValue
	,	rs.[Signals] RsSignals
	,	rs.[Value] RsValue
	,	prs.[Signals] PrsSignals
	,	prs.[Value] PrsValue
INTO #pivot
FROM [EodPrices] q
LEFT JOIN #PriceSignals vs ON vs.ShareId = q.[ShareId] AND vs.[Day] = q.[Day]		
LEFT JOIN #RsSignals rs ON rs.ShareId = q.[ShareId] AND rs.[Day] = q.[Day]		
LEFT JOIN #PeerRsSignals prs ON prs.ShareId = q.[ShareId] AND prs.[Day] = q.[Day]		
WHERE q.[Day] >= @cutoffDate

DECLARE @IsRising AS INT			= 0x0001; --       // Going up
DECLARE @IsFalling AS INT			= 0x0002; --       // Going down
DECLARE @DoubleTop AS INT			= 0x0004; --       // Double Bottom
DECLARE @DoubleBottom AS INT		= 0x0008; --       // Double Bottom
DECLARE @TripleTop AS INT			= 0x0010; --       // Triple Top
DECLARE @TripleBottom AS INT		= 0x0020; --       // Triple Bottom
DECLARE @AboveBullSupport AS INT	= 0x0040; --       // Current box is abobe bullish support level




select [ShareId]
	, [Day]
	, CONVERT(bit, IIF(PriceSignals&@IsRising=@IsRising, 1, 0)) AS Rising
	, CONVERT(bit, IIF(PriceSignals&@DoubleTop=@DoubleTop, 1, 0)) AS DoubleTop
	, CONVERT(bit, IIF(PriceSignals&@TripleTop=@TripleTop, 1, 0)) AS TripleTop
	, CONVERT(bit, IIF(RsSignals&@IsRising=@IsRising, 1, 0)) AS RsRising
	, CONVERT(bit, IIF(RsSignals&@DoubleTop=@DoubleTop, 1, 0)) AS RsBuy
	, CONVERT(bit, IIF(PrsSignals&@IsRising=@IsRising, 1, 0)) AS PeerRsRising
	, CONVERT(bit, IIF(PrsSignals&@DoubleTop=@DoubleTop, 1, 0)) AS PeerRsBuy
	, CONVERT(bit, IIF(PriceSignals&@IsFalling=@IsFalling, 1, 0)) AS Falling
	, CONVERT(bit, IIF(PriceSignals&@DoubleBottom=@DoubleBottom, 1, 0)) AS DoubleBottom
	, CONVERT(bit, IIF(PriceSignals&@TripleBottom=@TripleBottom, 1, 0)) AS TripleBottom
	, CONVERT(bit, IIF(RsSignals&@IsFalling=@IsFalling, 1, 0)) AS RsFalling
	, CONVERT(bit, IIF(RsSignals&@DoubleBottom=@DoubleBottom, 1, 0)) AS RsSell
	, CONVERT(bit, IIF(PrsSignals&@IsFalling=@IsFalling, 1, 0)) AS PeerRsFalling
	, CONVERT(bit, IIF(PrsSignals&@DoubleBottom=@DoubleBottom, 1, 0)) AS PeerRsSell
	, CONVERT(bit, IIF(PriceSignals&@AboveBullSupport=@AboveBullSupport, 1, 0)) AS AboveBullSupport
into #ShareResults
from #pivot

UPDATE [dbo].[ShareIndicators]
	SET [Rising] = sr.[Rising]
	,	[DoubleTop] = sr.[DoubleTop]
	,	[TripleTop] = sr.[TripleTop]
	,	[RsRising] = sr.[RsRising]
	,	[RsBuy] = sr.[RsBuy]
	,	[PeerRsRising] = sr.[PeerRsRising]
	,	[PeerRsBuy] = sr.[PeerRsBuy]
	,	Falling = sr.Falling
	,	[DoubleBottom] = sr.[DoubleBottom]
	,	[TripleBottom] = sr.[TripleBottom]
	,	[RsFalling] = sr.[RsFalling]
	,	[RsSell] = sr.[RsSell]
	,	[PeerRsFalling] = sr.[PeerRsFalling]
	,	[PeerRsSell] = sr.[PeerRsSell]
	,	[AboveBullSupport] = sr.[AboveBullSupport]
FROM [dbo].[ShareIndicators] si
INNER JOIN #ShareResults sr ON sr.[ShareId] = si.[ShareId] AND sr.[Day] = si.[Day]
WHERE si.[Day] >= @cutoffDate

INSERT INTO [dbo].[ShareIndicators] ([Id], [ShareId], [Day], [Rising], [DoubleTop], [TripleTop],[RsRising],[RsBuy],[PeerRsRising],[PeerRsBuy]
		, [Falling], [DoubleBottom], [TripleBottom], [RsFalling], [RsSell], [PeerRsFalling], [PeerRsSell], [AboveBullSupport])
	SELECT NEWID() AS Id
		, sr.[ShareId]
		, sr.[Day]
		, sr.[Rising]
		, sr.[DoubleTop]
		, sr.[TripleTop]
		, sr.[RsRising]
		, sr.[RsBuy]
		, sr.[PeerRsRising]
		, sr.[PeerRsBuy]
		, sr.[Falling]
		, sr.[DoubleBottom]
		, sr.[TripleBottom]
		, sr.[RsFalling]
		, sr.[RsSell]
		, sr.[PeerRsFalling]
		, sr.[PeerRsSell]
		, sr.[AboveBullSupport]
	FROM #ShareResults sr
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = sr.[ShareId] AND si.[Day] = sr.[Day]
	WHERE si.[Id] IS NULL


UPDATE si
		SET si.[ClosedAboveEma10] = IIF(p.[Close] > si.[Ema10], 1, 0)
		,	si.[ClosedAboveEma30] = IIF(p.[Close] > si.[Ema30], 1, 0)
		FROM [dbo].[ShareIndicators] si
		LEFT JOIN [dbo].[EodPrices] p 
		ON p.[ShareId] = si.[ShareId]
			AND p.[Day] = si.[Day]
		WHERE si.[CreatedAt] > @cutoffDate

-- Now update the Events code with notifications of new events.
SELECT ShareId, [Day], DoubleTop, TripleTop, RsBuy, PeerRsBuy, DoubleBottom, TripleBottom, RsSell, PeerRsSell, ClosedAboveEma10, ClosedAboveEma30, [AboveBullSupport]
		, ROW_NUMBER() OVER(PARTITION BY [ShareId] ORDER BY [Day] ASC) as Ordinal#
	INTO #today
	FROM [dbo].[ShareIndicators]
	WHERE [Day] >= @cutoffDate;


UPDATE [dbo].[ShareIndicators]
		SET [UpdatedAt] = GETDATE()
		,	[NewEvents] = iif(td.[DoubleTop]^yd.[DoubleTop]=1 and td.[DoubleTop]=1, 0x0001, 0)
		+ iif(td.[TripleTop]^yd.[TripleTop]=1 and td.[TripleTop]=1, 0x0002, 0)
		+ iif(td.[RsBuy]^yd.[RsBuy]=1 and td.[RsBuy]=1, 0x0004, 0) 
		+ iif(td.[PeerRsBuy]^yd.[PeerRsBuy]=1 and td.[PeerRsBuy]=1, 0x0008, 0)
		+ iif(td.[DoubleBottom]^yd.[DoubleBottom]=1 and td.[DoubleBottom]=1, 0x0010, 0)
		+ iif(td.[TripleBottom]^yd.[TripleBottom]=1 and td.[TripleBottom]=1, 0x0020, 0) 
		+ iif(td.[RsSell]^yd.[RsSell]=1 and td.[RsSell]=1, 0x0040, 0) 
		+ iif(td.[PeerRsSell]^yd.[PeerRsSell]=1 and td.[PeerRsSell]=1, 0x0080, 0)
		+ iif(td.[ClosedAboveEma10]^yd.[ClosedAboveEma10]=1 and td.[ClosedAboveEma10]=1, 0x0100, 0)
		+ iif(td.[ClosedAboveEma30]^yd.[ClosedAboveEma30]=1 and td.[ClosedAboveEma30]=1, 0x0200, 0)
		+ iif(td.[ClosedAboveEma10]^yd.[ClosedAboveEma10]=1 and yd.[ClosedAboveEma10]=1, 0x0400, 0)
		+ iif(td.[ClosedAboveEma30]^yd.[ClosedAboveEma30]=1 and yd.[ClosedAboveEma30]=1, 0x0800, 0) 
		+ iif(td.[AboveBullSupport]^yd.[AboveBullSupport]=1 and yd.[AboveBullSupport]=1, 0x1000, 0)
	FROM [dbo].[ShareIndicators] si
	LEFT JOIN #today td ON td.[ShareId] = si.[ShareId] AND td.[Day] = si.[Day]
	LEFT JOIN #today yd ON yd.[ShareId] = td.[ShareId] AND yd.Ordinal# = td.Ordinal#-1
	WHERE si.[Day] >= @cutoffDate and td.[Ordinal#] > 1

RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO



