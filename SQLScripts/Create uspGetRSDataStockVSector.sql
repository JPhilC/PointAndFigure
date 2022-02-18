uSE PnFData;  
GO

IF OBJECT_ID('dbo.uspGetRSDataStockVSector', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGetRSDataStockVSector;  
GO  

CREATE PROCEDURE [uspGetRSDataStockVSector] (
@tidm NVARCHAR(10),
@upToDate datetime)
AS
SET NOCOUNT ON
SELECT p.[Day], p.[Close]/ixv.[Value] * 1000 as [Value]
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector = s.SuperSector
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]
	WHERE p.ShareId IN (SELECT Id FROM Shares WHERE Tidm=@tidm)
		AND p.[Day]<=@uptoDate
		AND ixv.Value IS NOT NULL
	ORDER BY p.[Day] DESC

RETURN
