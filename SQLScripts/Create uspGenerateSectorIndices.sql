USE PnFData;  
GO

IF OBJECT_ID('dbo.uspGenerateSectorIndices', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGenerateSectorIndices;  
GO  

CREATE PROCEDURE uspGenerateSectorIndices
		@CutOffDate Date
	AS
SET NOCOUNT ON;
SET ANSI_WARNINGS OFF;
-- Generates a Equal Weighted Indexes by exchange code, exchange sub code and sector

RAISERROR (N'Generating sector indices ...', 0, 0) WITH NOWAIT;

--DECLARE @upToDate DATETIME;
--SET @upToDate = '2022-02-18';

IF object_id('tempdb..#totals','U') is not null
	DROP TABLE #totals;

IF object_id('tempdb..#newIndex','U') is not null
	DROP TABLE #newIndex;

IF object_id('tempdb..#avgCount','U') is not null
	DROP TABLE #avgCount;

SELECT s.[ExchangeCode], s.[ExchangeSubCode], s.[SuperSector], p.[Day]
		, SUM(p.[AdjustedClose]) TotalClose
		, COUNT(*) ShareCount 
		, SUM(CONVERT(FLOAT, New52WeekHigh)) Highs
		, SUM(CONVERT(FLOAT,New52WeekHigh) + CONVERT(FLOAT, New52WeekLow)) HighsAndLows
	INTO #totals
	FROM EodPrices p
	LEFT JOIN Shares s ON s.Id = p.ShareId
	WHERE p.[Day] <= @CutOffDate AND ISNULL(s.[SuperSector], '') <>''
	GROUP BY s.[ExchangeCode], s.[ExchangeSubCode], s.[SuperSector], p.[Day]
	ORDER BY s.[ExchangeCode], s.[ExchangeSubCode], s.[SuperSector], p.[Day] DESC;


-- Sanity check for dodgy data (e.g. Days a much lower number of contributors than usual)
SELECT [ExchangeCode], [ExchangeSubCode], [SuperSector], AVG(ShareCount) AvgCount
	INTO #avgCount
	FROM #totals
	GROUP BY [ExchangeCode], [ExchangeSubCode], [SuperSector]

DELETE #totals
	FROM #totals t
	INNER JOIN #avgCount ac ON ac.[ExchangeCode] = t.[ExchangeCode] 
		AND ac.[ExchangeSubCode] = t.[ExchangeSubCode]
		AND ac.[SuperSector] = t.[SuperSector]
	WHERE t.[ShareCount] < ac.[AvgCount]*0.25 


---- Create any new indexes
INSERT INTO [Indices] ([ExchangeCode], [ExchangeSubCode], [SuperSector])
	SELECT DISTINCT 
		t.[ExchangeCode], 
		t.[ExchangeSubCode],
		t.[SuperSector]
	FROM #totals t
	LEFT JOIN [Indices] i ON i.[ExchangeCode] = t.[ExchangeCode]
		AND i.[ExchangeSubCode] = t.[ExchangeSubCode]
		AND ISNULL(i.[SuperSector], '') <>''
		AND i.[SuperSector] = t.[SuperSector]
	WHERE i.Id IS NULL;
	

UPDATE iv
	SET iv.[Value] = t.[TotalClose]
	,	iv.[Contributors] = t.[ShareCount]
	,	iv.[PercentHighLow] = IIF(t.[HighsAndLows] > 0, CONVERT(FLOAT, (t.[Highs]/t.[HighsAndLows])*100.0) , 0)
	,	iv.[UpdatedAt] = GETDATE()
FROM [dbo].[IndexValues] iv
LEFT JOIN [Indices] i ON i.[Id] = iv.[IndexId]
LEFT JOIN #totals t ON t.[ExchangeCode] = i.[ExchangeCode]
	AND t.[ExchangeSubCode] = i.[ExchangeSubCode]
	AND t.[SuperSector] = i.[SuperSector]
	AND t.[Day] = iv.[Day]
WHERE t.[TotalClose] IS NOT NULL


INSERT INTO [IndexValues] ([IndexId], [Day], [Value], [Contributors], [PercentHighLow])
	SELECT i.[id], t.[Day], t.[TotalClose], t.[ShareCount]
			, IIF(t.[HighsAndLows] > 0, CONVERT(FLOAT, (t.[Highs]/t.[HighsAndLows])*100.0) , 0)
		FROM #totals t
		LEFT JOIN [Indices] i ON i.[ExchangeCode] = t.[ExchangeCode]
			AND i.[ExchangeSubCode] = t.[ExchangeSubCode]
			AND i.[SuperSector] = t.[SuperSector]
		LEFT JOIN [IndexValues] iv ON iv.[IndexId] = i.Id AND iv.[Day] = t.[Day]
		WHERE iv.[id] IS NULL


DROP TABLE #totals;
DROP TABLE #avgCount;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;

RETURN

