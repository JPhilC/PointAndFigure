uSE PnFData;  
GO

IF OBJECT_ID('dbo.uspGetRSDataSuperSectorVMarket', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGetRSDataSuperSectorVMarket;  
GO  

CREATE PROCEDURE [uspGetRSDataSuperSectorVMarket] (
@exchangeCode NVARCHAR(10),
@exchangeSubCode NVARCHAR(4),
@superSector NVARCHAR(100),
@upToDate datetime)
AS
SET NOCOUNT ON

IF object_id('tempdb..#market','U') is not null
	DROP TABLE #market;

-- Get market prices
SELECT ixv.[Day], ixv.[Value]
	INTO #market
	FROM IndexValues ixv
	WHERE ixv.IndexId IN (SELECT ix.Id FROM Indices ix WHERE ix.ExchangeCode = @exchangeCode AND ix.ExchangeSubCode = @exchangeSubCode and ix.[SuperSector] IS NULL)
		AND ixv.[Day] <= @upToDate;


SELECT ixsv.[Day], ixsv.[Value]/m.[Value] * 1000 as [Value]
	FROM IndexValues ixsv
	LEFT JOIN #market m ON m.[Day] = ixsv.[Day]
	WHERE ixsv.IndexId IN (SELECT ixs.[Id] FROM Indices ixs WHERE ixs.ExchangeCode = @exchangeCode AND ixs.ExchangeSubCode = @exchangeSubCode AND ixs.[SuperSector] = @superSector)
		AND ixsv.[Day]<=@uptoDate
	ORDER BY ixsv.[Day] DESC

DROP TABLE #market

RETURN
