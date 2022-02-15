USE PnFData;  
GO

IF OBJECT_ID('dbo.uspGetSubCodeSectorIndex', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGetSubCodeSectorIndex;  
GO  

CREATE PROCEDURE [uspGetSubCodeSectorIndex] (
@subCode NVARCHAR(4),
@sector NVARCHAR(60),
@upToDate datetime)
AS
--SET @subCode = 'AIM';
--SET @uptoDate = '2022-02-04';
SET NOCOUNT ON;

SELECT p.[Day], SUM(p.[Close]) TotalClose, COUNT(*) ShareCount 
	INTO #tempOne
	FROM EodPrices p 
	WHERE p.[ShareId] IN (SELECT s.[Id] FROM [Shares] s WHERE s.[ExchangeSubCode] = @subCode AND s.[Sector]=@sector)
		AND p.[Day] <= @upToDate
	GROUP BY p.[Day];

SELECT [Day], ([TotalClose]/[ShareCount]) IndexValue, ShareCount
	FROM #tempOne
	ORDER BY [Day] DESC

DROP TABLE #tempOne;

RETURN
GO

-- DELETE FROM EodPrices WHERE Day='2017-01-02 00:00:00.0000000'
