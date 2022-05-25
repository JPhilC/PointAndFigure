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

IF object_id('tempdb..#IndexSignals','U') is not null
	DROP TABLE #IndexSignals;

IF object_id('tempdb..#RsSignals','U') is not null
	DROP TABLE #RsSignals;

IF object_id('tempdb..#Pivot','U') is not null
	DROP TABLE #Pivot;

IF object_id('tempdb..#IndexResults','U') is not null
	DROP TABLE #IndexResults;

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #IndexSignals
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 1

SELECT ic.[IndexId]
	,	s.[Day]
	,	s.[Signals]
	,	s.[Value]
	INTO #RsSignals
	FROM [PnFCharts] c
	JOIN [IndexCharts] ic ON ic.[ChartId] = c.[Id]
	LEFT JOIN [PnFSignals] s ON s.[PnFChartId] = c.[Id]
	WHERE c.[Source] = 4

SELECT ixs.[IndexId]
	,	ixs.[Day]
	,	ixs.[Signals] IndexSignals
	,	ixs.[Value] IndexValue
	,	rs.[Signals] RsSignals
	,	rs.[Value] RsValue
INTO #pivot
FROM #IndexSignals ixs 		
LEFT JOIN #RsSignals rs ON rs.IndexId = ixs.[IndexId] AND rs.[Day] = ixs.[Day]		

DECLARE @IsRising AS INT			= 0x0001; --       // Going up
DECLARE @IsFalling AS INT			= 0x0002; --       // Going down
DECLARE @DoubleTop AS INT			= 0x0004; --       // Double Bottom
DECLARE @DoubleBottom AS INT		= 0x0008; --       // Double Bottom
DECLARE @TripleTop AS INT			= 0x0010; --       // Triple Top
DECLARE @TripleBottom AS INT		= 0x0020; --       // Triple Bottom
DECLARE @AboveBullSupport AS INT	= 0x0040; --       // Current box is abobe bullish support level


select [IndexId]
	, [Day]
	, CONVERT(bit, IIF(IndexSignals&@IsRising=@IsRising, 1, 0)) AS Rising
	, CONVERT(bit, IIF(IndexSignals&@DoubleTop=@DoubleTop, 1, 0)) AS Buy
	, CONVERT(bit, IIF(RsSignals&@IsRising=@IsRising, 1, 0)) AS RsRising
	, CONVERT(bit, IIF(RsSignals&@DoubleTop=@DoubleTop, 1, 0)) AS RsBuy
	, CONVERT(bit, IIF(IndexSignals&@IsFalling=@IsFalling, 1, 0)) AS Falling
	, CONVERT(bit, IIF(IndexSignals&@DoubleBottom=@DoubleBottom, 1, 0)) AS Sell
	, CONVERT(bit, IIF(RsSignals&@IsFalling=@IsFalling, 1, 0)) AS RsFalling
	, CONVERT(bit, IIF(RsSignals&@DoubleBottom=@DoubleBottom, 1, 0)) AS RsSell
into #IndexResults
from #pivot



UPDATE [dbo].[IndexIndicators]
	SET [Rising] = sr.[Rising]
	,	[Buy] = sr.[Buy]
	,	[RsRising] = sr.[RsRising]
	,	[RsBuy] = sr.[RsBuy]
	,	Falling = sr.Falling
	,	[Sell] = sr.[Sell]
	,	[RsFalling] = sr.[RsFalling]
	,	[RsSell] = sr.[RsSell]
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