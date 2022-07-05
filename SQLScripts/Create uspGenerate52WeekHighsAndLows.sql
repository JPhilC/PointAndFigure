USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerate52WeekHighsAndLows]    Script Date: 13/03/2022 17:38:53 ******/
DROP PROCEDURE [dbo].[uspGenerate52WeekHighsAndLows]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerate52WeekHighsAndLows]    Script Date: 13/03/2022 17:38:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGenerate52WeekHighsAndLows] 
	AS
SET NOCOUNT ON

-- Use 258 to represent 52 weeks
IF OBJECT_ID('tempdb..#highlows') IS NOT NULL BEGIN
    DROP TABLE #highlows
END

IF OBJECT_ID('tempdb..#today') IS NOT NULL BEGIN
    DROP TABLE #today
END

IF OBJECT_ID('tempdb..#newvalues') IS NOT NULL BEGIN
    DROP TABLE #newvalues
END

RAISERROR (N'Calculating 52 week highs and lows ...', 0, 0) WITH NOWAIT;

SELECT [ShareId]
	,	[Day]
	,	[AdjustedClose]
	,	MIN([AdjustedClose]) OVER (
			PARTITION BY [ShareId]
			ORDER BY [ShareId], [Day]
			ROWS BETWEEN 258 PRECEDING AND CURRENT ROW
			) Low52w
	,	MAX([AdjustedClose]) OVER (
			PARTITION BY [ShareId]
			ORDER BY [ShareId], [Day]
			ROWS BETWEEN 258 PRECEDING AND CURRENT ROW
			) High52w
	INTO #highlows
	FROM [EodPrices]
	WHERE [Day] > DATEADD(YEAR, -1, GETDATE())
	ORDER BY [ShareId], [Day] DESC

RAISERROR (N'Updating share 52 week highs and lows ...', 0, 0) WITH NOWAIT;

UPDATE [EodPrices]
	SET [High52Week] = hl.[High52w]
	,	[Low52Week] = hl.[Low52w]
FROM [EodPrices] q
LEFT JOIN #highlows hl ON hl.[ShareId] = q.[ShareId] AND hl.[Day] = q.[Day]
-- WHERE q.[High52Week] IS NULL

DROP TABLE #highlows;

RAISERROR (N'Updating share new high and new low flags ...', 0, 0) WITH NOWAIT;

SELECT q.[ShareId]
	,	q.[Day]
	,	q.[Id]
	,	q.[AdjustedClose]
	,	q.[High52Week]
	,	q.[Low52Week]
	,	ROW_NUMBER() OVER(PARTITION BY q.[ShareId] ORDER BY q.[Day] ASC) as Ordinal#
INTO #today
FROM [EodPrices] q
WHERE q.[Day] > DATEADD(YEAR, -1, GETDATE())

SELECT td.[Id]
		,	IIF(td.[AdjustedClose] > yd.[High52Week], 1, 0) AS New52WeekHigh
		,	IIF(td.[AdjustedClose] < yd.[Low52Week], 1, 0) AS New52WeekLow
	INTO #newValues
	FROM #today td
	LEFT JOIN #today yd ON yd.[ShareId] = td.[ShareId] and yd.[Ordinal#] = td.[Ordinal#]-1
	

UPDATE [EodPrices]
	SET [New52WeekHigh] = nv.[New52WeekHigh]
	,	[New52WeekLow] = nv.[New52WeekLow]
FROM [EodPrices] q
LEFT JOIN #newValues nv on nv.[Id] = q.[Id]
-- WHERE q.[New52WeekHigh] IS NULL

DROP TABLE #today;
DROP TABLE #newValues;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;

RETURN

GO
	



