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



-- Sectors
SELECT i.[Id] IndexId, si.[Day] 
		, count(*) [Members]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsBuy], 0))) [RsBuy]
		, CONVERT(FLOAT,SUM(COALESCE(si.[RsRising], 0))) [RsRising]
		, CONVERT(FLOAT, SUM(IIF(si.[DoubleTop]=1,1,0))) [DoubleTop] 
		, CONVERT(FLOAT,SUM(COALESCE(si.[ClosedAboveEma10], 0))) [Percent10]
		, CONVERT(FLOAT, SUM(COALESCE(si.[ClosedAboveEma30], 0))) [Percent30]
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
	INTO #Percentages
	FROM #Sectors i
	ORDER BY i.IndexId, i.[Day] DESC


UPDATE iv
	SET iv.[PercentRsBuy] = p.[PercentRsBuy]
	,   iv.[PercentRsRising] = p.[PercentRsRising]
	,   iv.[BullishPercent] = p.[BullishPercent]
	,	iv.[PercentAboveEma10] = p.[Percent10]
	,	iv.[PercentAboveEma30] = p.[Percent30]
FROM [dbo].[IndexValues] iv
INNER JOIN #Percentages p
ON p.[IndexId] = iv.[IndexId]
	AND p.[Day] = iv.[Day];

INSERT INTO [dbo].[IndexValues] ([Id], [IndexId], [Day], [PercentRsBuy], [PercentRsRising], [BullishPercent], [PercentAboveEma10], [PercentAboveEma30])
	SELECT NEWID() AS Id
		, p.[IndexId]
		, p.[Day]
		, p.[PercentRsBuy]
		, p.[PercentRsRising]
		, p.[BullishPercent]
		, p.[Percent10]
		, p.[Percent30]
	FROM #Percentages p
	LEFT JOIN [IndexIndicators] ii ON ii.[IndexId] = p.[IndexId] AND ii.[Day] = p.[Day]
	WHERE ii.[Id] IS NULL

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
	INTO #MPercentages
	FROM #Markets i
	ORDER BY i.[IndexId], i.[Day] DESC

UPDATE iv
	SET iv.[PercentRsBuy] = p.[PercentRsBuy]
	,   iv.[PercentRsRising] = p.[PercentRsRising]
	,   iv.[BullishPercent] = p.[BullishPercent]
	,	iv.[PercentAboveEma10] = p.[Percent10]
	,	iv.[PercentAboveEma30] = p.[Percent30]
FROM [dbo].[IndexValues] iv
INNER JOIN #MPercentages p
ON p.[IndexId] = iv.[IndexId]
	AND p.[Day] = iv.[Day];

INSERT INTO [dbo].[IndexValues] ([Id], [IndexId], [Day], [PercentRsBuy], [PercentRsRising], [BullishPercent], [PercentAboveEma10], [PercentAboveEma30])
	SELECT NEWID() AS Id
		, p.[IndexId]
		, p.[Day]
		, p.[PercentRsBuy]
		, p.[PercentRsRising]
		, p.[BullishPercent]
		, p.[Percent10]
		, p.[Percent30]
	FROM #MPercentages p
	LEFT JOIN [IndexIndicators] ii ON ii.[IndexId] = p.[IndexId] AND ii.[Day] = p.[Day]
	WHERE ii.[Id] IS NULL

DROP TABLE #Markets
DROP TABLE #MPercentages

RAISERROR (N'Done.', 0, 0) WITH NOWAIT;

RETURN

GO



