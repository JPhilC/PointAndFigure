USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspUpdateBullishIndicators]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspUpdateBullishIndicators] 
	AS
SET NOCOUNT ON
RAISERROR (N'Updating bullish indicators ...', 0, 0) WITH NOWAIT;

-- Get the latest day from EodPrices
DECLARE @Day Date
SELECT @Day = CONVERT(Date, MAX([Day]))
	FROM [PnFData].[dbo].[ShareIndicators]

-- Sectors
SELECT i.[Id] IndexId, si.[Day] 
		, count(*) [Members]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsBuy], 0))) [RsBuy]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsRising], 0))) [RsRising]
		, CONVERT(FLOAT, SUM(IIF(si.[DoubleTop]=1,1,0))) [DoubleTop] 
		, CONVERT(FLOAT,SUM(COALESCE(si.[ClosedAboveEma10], 0))) [Percent10]
		, CONVERT(FLOAT, SUM(COALESCE(si.[ClosedAboveEma30], 0))) [Percent30]
		, CONVERT(FLOAT, SUM(COALESCE(si.[AboveBullSupport], 0))) [AboveBullSupport]
    INTO #Sectors
	FROM [PnFData].[dbo].[Indices] i
	RIGHT OUTER JOIN [PnFData].[dbo].[Shares] s on s.ExchangeSubCode = i.ExchangeSubCode and s.SuperSector = i.SuperSector
	RIGHT OUTER JOIN [PnFData].[dbo].[ShareIndicators] si on si.ShareId = s.Id 
    WHERE i.[SuperSector] IS NOT NULL
	GROUP BY i.Id, si.[Day]
	ORDER BY i.Id, si.[Day] DESC

SELECT i.[IndexId], i.[Day], i.[Members]
		, i.[RsRising]/i.[Members]*100 [PercentRsRising]
		, i.[RsBuy]/i.[Members]*100 [PercentRsBuy]
		, i.[DoubleTop]/i.[Members]*100 [BullishPercent]
		, i.[Percent10]/i.[Members]*100 [Percent10]
		, i.[Percent30]/i.[Members]*100 [Percent30]
		, i.[AboveBullSupport]/i.[Members] * 100 [PercentPositiveTrend]
	INTO #Percentages
	FROM #Sectors i
	ORDER BY i.IndexId, i.[Day] DESC


UPDATE iv
	SET iv.[PercentRsBuy] = p.[PercentRsBuy]
	,   iv.[PercentRsRising] = p.[PercentRsRising]
	,   iv.[BullishPercent] = p.[BullishPercent]
	,	iv.[PercentAboveEma10] = p.[Percent10]
	,	iv.[PercentAboveEma30] = p.[Percent30]
	,	iv.[PercentPositiveTrend] = p.[PercentPositiveTrend]
FROM [dbo].[IndexValues] iv
INNER JOIN #Percentages p
ON p.[IndexId] = iv.[IndexId]
	AND p.[Day] = iv.[Day];

DROP TABLE #Sectors
DROP TABLE #Percentages

-- Markets
SELECT i.[Id] IndexId, si.[Day] 
		, count(*) [Members]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsBuy], 0))) [RsBuy]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsRising], 0))) [RsRising]
		, CONVERT(FLOAT, SUM(IIF(si.[DoubleTop]=1,1,0))) [DoubleTop] 
		, CONVERT(FLOAT,SUM(COALESCE(si.[ClosedAboveEma10], 0))) [Percent10]
		, CONVERT(FLOAT, SUM(COALESCE(si.[ClosedAboveEma30], 0))) [Percent30]
		, CONVERT(FLOAT, SUM(COALESCE(si.[AboveBullSupport], 0))) [AboveBullSupport]
    INTO #Markets
	FROM [PnFData].[dbo].[Indices] i
	RIGHT OUTER JOIN [PnFData].[dbo].[Shares] s on s.ExchangeSubCode = i.ExchangeSubCode
	RIGHT OUTER JOIN [PnFData].[dbo].[ShareIndicators] si on si.ShareId = s.Id  
    WHERE i.[SuperSector] IS NULL
	GROUP BY i.[Id], si.[Day]
	ORDER BY i.[Id], si.[Day] DESC


SELECT i.[IndexId], i.[Day], i.[Members]
		, i.[RsRising]/i.[Members]*100 [PercentRsRising]
		, i.[RsBuy]/i.[Members]*100 [PercentRsBuy]
		, i.[DoubleTop]/i.[Members]*100 [BullishPercent]
		, i.[Percent10]/i.[Members]*100 [Percent10]
		, i.[Percent30]/i.[Members]*100 [Percent30]		
		, i.[AboveBullSupport]/i.[Members] * 100 [PercentPositiveTrend]
	INTO #MPercentages
	FROM #Markets i
	ORDER BY i.[IndexId], i.[Day] DESC

UPDATE iv
	SET iv.[PercentRsBuy] = p.[PercentRsBuy]
	,   iv.[PercentRsRising] = p.[PercentRsRising]
	,   iv.[BullishPercent] = p.[BullishPercent]
	,	iv.[PercentAboveEma10] = p.[Percent10]
	,	iv.[PercentAboveEma30] = p.[Percent30]
	,	iv.[PercentPositiveTrend] = p.[PercentPositiveTrend]
FROM [dbo].[IndexValues] iv
INNER JOIN #MPercentages p
ON p.[IndexId] = iv.[IndexId]
	AND p.[Day] = iv.[Day];

DROP TABLE #Markets
DROP TABLE #MPercentages

RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO



