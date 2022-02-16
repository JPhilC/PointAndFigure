USE PnFData;  
GO

IF OBJECT_ID('dbo.uspGenerateSectorIndices', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGenerateSectorIndices;  
GO  

CREATE PROCEDURE uspGenerateSectorIndices (@upToDate DATETIME)
	AS
SET NOCOUNT ON;
-- Generates a Equal Weighted Indexes by exchange code, exchange sub code and sector

--DECLARE @upToDate DATETIME;
--SET @upToDate = '2022-02-04';

IF object_id('tempdb..#totals','U') is not null
	DROP TABLE #totals;

IF object_id('tempdb..#weights','U') is not null
	DROP TABLE #weights;

	
IF object_id('tempdb..#newIndex','U') is not null
	DROP TABLE #newIndex;

SELECT s.[ExchangeCode], s.[ExchangeSubCode], s.[Sector], p.[Day], SUM(p.[Close]) TotalClose, COUNT(*) ShareCount 
	INTO #totals
	FROM EodPrices p
	LEFT JOIN Shares s ON s.Id = p.ShareId
	WHERE p.[Day] <= @upToDate AND ISNULL(s.[Sector], '') <>''
	GROUP BY s.[ExchangeCode], s.[ExchangeSubCode], s.[Sector], p.[Day]
	ORDER BY s.[ExchangeCode], s.[ExchangeSubCode], s.[Sector], p.[Day] DESC;


-- Weights for full/AIM (equal weighting)
SELECT t.[ExchangeCode], t.[ExchangeSubCode], t.[Sector], p.[ShareId], p.[Day], (1/t.[ShareCount]*100) [Weight]
	INTO #weights
	FROM [EodPrices] p
	LEFT JOIN [Shares] s ON s.[Id] = p.[ShareId]
	LEFT JOIN #totals t on t.[ExchangeCode] = s.[ExchangeCode] 
		AND t.[ExchangeSubCode] = s.[ExchangeSubCode] 
		AND t.[Sector] = s.[Sector] 
		AND t.[Day] = p.[Day]
	WHERE p.[Day] <= @upToDate AND ISNULL(s.[Sector], '') <>''

SELECT w.[ExchangeCode], w.[ExchangeSubCode], w.[Sector], w.[Day], SUM(w.[Weight] * p.[Close]) [Value], Count(*) [ShareCount]
	INTO #newIndex
	FROM #weights w
	LEFT JOIN [EodPrices] p ON p.ShareId = w.[ShareId] and p.[Day] = w.[Day]
	LEFT JOIN [Shares] s ON s.[Id] = w.[ShareId]
	GROUP BY w.[ExchangeCode], w.[ExchangeSubCode], w.[Sector], w.[Day]
	ORDER BY w.[ExchangeCode], w.[ExchangeSubCode], w.[Sector], w.[Day] DESC

-- Delete existing indexes
DELETE [Indices]
WHERE [id] IN
	(SELECT i.[Id] 
		FROM [Indices] i
		WHERE ISNULL(i.[Sector], '') <> ''
			AND i.[ExchangeCode] IN (SELECT DISTINCT w.[ExchangeCode] FROM #weights w)
			AND i.[ExchangeSubCode] IN (SELECT DISTINCT w.[ExchangeSubCode] FROM #weights w)
			AND i.[Sector] IN (SELECT DISTINCT w.[Sector] FROM #weights w)
			);

-- Create new indexes
INSERT INTO [Indices] ([ExchangeCode], [ExchangeSubCode], [Sector])
	SELECT DISTINCT 
	[ExchangeCode], 
	[ExchangeSubCode],
	[Sector]
	FROM #newIndex;


INSERT INTO [IndexValues] ([IndexId], [Day], [Value], [Contributors])
	SELECT i.[id], ni.[Day], ni.[Value], ni.[ShareCount]
		FROM #newIndex ni
		LEFT JOIN [Indices] i ON i.[ExchangeCode] = ni.[ExchangeCode]
			AND i.[ExchangeSubCode] = ni.[ExchangeSubCode]
			AND i.[Sector] = NI.[Sector]


DROP TABLE #totals;
DROP TABLE #weights;
DROP TABLE #newIndex


RETURN
