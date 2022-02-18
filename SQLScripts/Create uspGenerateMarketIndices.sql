USE PnFData;  
GO

IF OBJECT_ID('dbo.[uspGenerateMarketIndices]', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.[uspGenerateMarketIndices];  
GO  

CREATE PROCEDURE [uspGenerateMarketIndices] (@upToDate DATETIME)
	AS
SET NOCOUNT ON;
-- Generates a Price Weighted Index by exchange code and exchange sub code

--DECLARE @upToDate DATETIME;
--SET @upToDate = '2022-02-04';

IF object_id('tempdb..#totals','U') is not null
	DROP TABLE #totals;

IF object_id('tempdb..#weights','U') is not null
	DROP TABLE #weights;

	
IF object_id('tempdb..#newIndex','U') is not null
	DROP TABLE #newIndex;

-- Excluding sector (i.e. for complete full/AIM markets
SELECT s.[ExchangeCode], s.[ExchangeSubCode], p.[Day], SUM(p.[Close]) TotalClose, COUNT(*) ShareCount 
	INTO #totals
	FROM EodPrices p
	LEFT JOIN Shares s ON s.Id = p.ShareId
	WHERE p.[Day] <= @upToDate AND ISNULL(s.[SuperSector], '') <>''
	GROUP BY s.[ExchangeCode], s.[ExchangeSubCode], p.[Day]
	ORDER BY s.[ExchangeCode], s.[ExchangeSubCode], P.[Day] DESC;

-- Weights for full/AIM
SELECT t.[ExchangeCode], t.[ExchangeSubCode], p.[ShareId], p.[Day], p.[Close]/t.[TotalClose] [Weight]
	INTO #weights
	FROM [EodPrices] p
	LEFT JOIN [Shares] s ON s.[Id] = p.[ShareId]
	LEFT JOIN #totals t on t.[ExchangeCode] = s.[ExchangeCode] AND t.[ExchangeSubCode] = s.[ExchangeSubCode] AND t.[Day] = p.[Day]
	WHERE p.[Day] <= @upToDate AND ISNULL(s.[SuperSector], '') <>''

SELECT w.[ExchangeCode], w.[ExchangeSubCode], w.[Day], SUM(w.[Weight] * p.[Close]) [Value], Count(*) [ShareCount]
	INTO #newIndex
	FROM #weights w
	LEFT JOIN [EodPrices] p ON p.ShareId = w.[ShareId] and p.[Day] = w.[Day]
	LEFT JOIN [Shares] s ON s.[Id] = w.[ShareId]
	GROUP BY w.[ExchangeCode], w.[ExchangeSubCode], w.[Day]
	ORDER BY w.[ExchangeCode], w.[ExchangeSubCode], w.[Day] DESC

-- Delete existing indexes
DELETE [Indices]
WHERE [id] IN
	(SELECT i.[Id] 
		FROM [Indices] i
		WHERE ISNULL(i.[SuperSector], '') = ''
			AND i.[ExchangeCode] IN (SELECT DISTINCT w.[ExchangeCode] FROM #weights w)
			AND i.[ExchangeSubCode] IN (SELECT DISTINCT w.[ExchangeSubCode] FROM #weights w)
			);

-- Create new indexes
INSERT INTO [Indices] ([ExchangeCode], [ExchangeSubCode])
	SELECT DISTINCT [ExchangeCode], [ExchangeSubCode] FROM #newIndex;


INSERT INTO [IndexValues] ([IndexId], [Day], [Value], [Contributors])
	SELECT i.[id], ni.[Day], ni.[Value], ni.[ShareCount]
		FROM #newIndex ni
		LEFT JOIN [Indices] i ON i.[ExchangeCode] = ni.[ExchangeCode]
			AND i.[ExchangeSubCode] = ni.[ExchangeSubCode]
			AND i.[SuperSector] IS NULL


DROP TABLE #totals;
DROP TABLE #weights;
DROP TABLE #newIndex


RETURN

