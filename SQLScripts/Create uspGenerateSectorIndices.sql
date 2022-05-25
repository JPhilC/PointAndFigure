USE PnFData;  
GO

IF OBJECT_ID('dbo.uspGenerateSectorIndices', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGenerateSectorIndices;  
GO  

CREATE PROCEDURE uspGenerateSectorIndices
		@CutOffDate Date
	AS
SET NOCOUNT ON;
-- Generates a Equal Weighted Indexes by exchange code, exchange sub code and sector

RAISERROR (N'Generating sector indices ...', 0, 0) WITH NOWAIT;

--DECLARE @upToDate DATETIME;
--SET @upToDate = '2022-02-18';

IF object_id('tempdb..#totals','U') is not null
	DROP TABLE #totals;

IF object_id('tempdb..#weights','U') is not null
	DROP TABLE #weights;

	
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


-- Weights for full/AIM (equal weighting)
SELECT t.[ExchangeCode], t.[ExchangeSubCode], t.[SuperSector], p.[ShareId], p.[Day], (10.0/t.[ShareCount]) [Weight]
	INTO #weights
	FROM [EodPrices] p
	LEFT JOIN [Shares] s ON s.[Id] = p.[ShareId]
	LEFT JOIN #totals t on t.[ExchangeCode] = s.[ExchangeCode] 
		AND t.[ExchangeSubCode] = s.[ExchangeSubCode] 
		AND t.[SuperSector] = s.[SuperSector] 
		AND t.[Day] = p.[Day]
	WHERE p.[Day] <= @CutOffDate AND ISNULL(s.[SuperSector], '') <>''

SELECT w.[ExchangeCode], w.[ExchangeSubCode], w.[SuperSector], w.[Day], SUM(w.[Weight] * p.[AdjustedClose]) [Value], Count(*) [ShareCount]
	INTO #newIndex
	FROM #weights w
	LEFT JOIN [EodPrices] p ON p.ShareId = w.[ShareId] and p.[Day] = w.[Day]
	LEFT JOIN [Shares] s ON s.[Id] = w.[ShareId]
	GROUP BY w.[ExchangeCode], w.[ExchangeSubCode], w.[SuperSector], w.[Day]
	ORDER BY w.[ExchangeCode], w.[ExchangeSubCode], w.[SuperSector], w.[Day] DESC


-- Sanity check for dodgy data (e.g. Days a much lower number of contributors than usual)
SELECT [ExchangeCode], [ExchangeSubCode], [SuperSector], AVG(ShareCount) AvgCount
	INTO #avgCount
	FROM #newIndex
	GROUP BY [ExchangeCode], [ExchangeSubCode], [SuperSector]

DELETE #newIndex
	FROM #newIndex ni
	INNER JOIN #avgCount ac ON ac.[ExchangeCode] = ni.[ExchangeCode] 
		AND ac.[ExchangeSubCode] = ni.[ExchangeSubCode]
		AND ac.[SuperSector] = ni.[SuperSector]
	WHERE ni.[ShareCount] < ac.[AvgCount]*0.25 


---- Create any new indexes
INSERT INTO [Indices] ([ExchangeCode], [ExchangeSubCode], [SuperSector])
	SELECT DISTINCT 
		ni.[ExchangeCode], 
		ni.[ExchangeSubCode],
		ni.[SuperSector]
	FROM #newIndex ni
	LEFT JOIN [Indices] i ON i.[ExchangeCode] = ni.[ExchangeCode]
		AND i.[ExchangeSubCode] = ni.[ExchangeSubCode]
		AND ISNULL(i.[SuperSector], '') <>''
		AND i.[SuperSector] = ni.[SuperSector]
	WHERE i.Id IS NULL;
	

UPDATE iv
	SET iv.[Value] = ni.[Value]
	,	iv.[Contributors] = ni.[ShareCount]
	,	iv.[PercentHighLow] = IIF(t.[HighsAndLows] > 0, CONVERT(FLOAT, (t.[Highs]/t.[HighsAndLows])*100.0) , 0)
	,	iv.[UpdatedAt] = GETDATE()
FROM [dbo].[IndexValues] iv
LEFT JOIN [Indices] i ON i.[Id] = iv.[IndexId]
LEFT JOIN #newIndex ni ON ni.[ExchangeCode] = i.[ExchangeCode]
	AND ni.[ExchangeSubCode] = i.[ExchangeSubCode]
	AND ni.[SuperSector] = i.[SuperSector]
	AND  ni.[Day] = iv.[Day]
LEFT JOIN #totals t ON t.[ExchangeCode] = i.[ExchangeCode]
	AND t.[ExchangeSubCode] = i.[ExchangeSubCode]
	AND t.[SuperSector] = i.[SuperSector]
	AND t.[Day] = iv.[Day]
WHERE ni.[Value] IS NOT NULL


INSERT INTO [IndexValues] ([IndexId], [Day], [Value], [Contributors], [PercentHighLow])
	SELECT i.[id], ni.[Day], ni.[Value], ni.[ShareCount]
			, IIF(t.[HighsAndLows] > 0, CONVERT(FLOAT, (t.[Highs]/t.[HighsAndLows])*100.0) , 0)
		FROM #newIndex ni
		LEFT JOIN #totals t ON t.[ExchangeCode] = ni.[ExchangeCode]
			AND t.[ExchangeSubCode] = ni.[ExchangeSubCode]
			AND t.[SuperSector] = ni.[SuperSector]
			AND t.[Day] = ni.[Day]
		LEFT JOIN [Indices] i ON i.[ExchangeCode] = ni.[ExchangeCode]
			AND i.[ExchangeSubCode] = ni.[ExchangeSubCode]
			AND i.[SuperSector] = ni.[SuperSector]
		LEFT JOIN [IndexValues] iv ON iv.[IndexId] = i.Id AND iv.[Day] = ni.[Day]
		WHERE iv.[id] IS NULL


DROP TABLE #totals;
DROP TABLE #weights;
DROP TABLE #newIndex;
DROP TABLE #avgCount;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;

RETURN

