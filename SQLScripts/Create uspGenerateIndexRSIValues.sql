USE [PnFData]
GO

IF OBJECT_ID('dbo.[uspGenerateIndexRSIValues]', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.[uspGenerateIndexRSIValues];  
GO  

/****** Object:  StoredProcedure [dbo].[uspGenerateMarketIndices]    Script Date: 05/03/2022 21:30:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGenerateIndexRSIValues]
		@CutOffDate DATE
	AS
SET NOCOUNT ON;

RAISERROR (N'Generating index RSI values ...', 0, 0) WITH NOWAIT;

-- Get market prices
SELECT ixh.ExchangeCode, ixh.ExchangeSubCode, ixv.[Day], ixv.[Value]
	INTO #market
	FROM IndexValues ixv
	LEFT JOIN Indices ixh ON ixh.Id = ixv.IndexId
	WHERE ixv.[Day] <= @CutOffDate AND ixv.IndexId IN (SELECT ix.Id FROM Indices ix WHERE ix.[SuperSector] IS NULL)
	ORDER BY ixh.ExchangeCode, ixh.ExchangeSubCode, ixv.[Day]



SELECT NEWID() as Id, ixsv.[Day], ixsv.[Value]/m.[Value] * 1000 as [Value], ixh.Id AS IndexId
	INTO #rsivalues
	FROM IndexValues ixsv
	LEFT JOIN Indices ixh ON ixh.Id = ixsv.IndexId
	LEFT JOIN #market m ON m.ExchangeCode = ixh.ExchangeCode AND m.ExchangeSubCode = ixh.ExchangeSubCode AND m.[Day] = ixsv.[Day]
	WHERE ixsv.[Day] <= @CutOffDate AND ixsv.IndexId IN (SELECT ixs.[Id] FROM Indices ixs WHERE ixs.[SuperSector] IS NOT NULL)
	ORDER BY ixh.Id, [Day]

-- Update existing records (within the last month)
UPDATE iv
	SET iv.[Value] = nv.[Value]
FROM [dbo].[IndexRSIValues] iv
LEFT JOIN [Indices] i ON i.[Id] = iv.[IndexId]
LEFT JOIN #rsivalues nv ON nv.[IndexId] = iv.[IndexId]
	AND nv.[Day] = iv.[Day]
WHERE nv.[Value] IS NOT NULL AND nv.[Value]<>iv.[Value]

-- Insert any new records
INSERT INTO IndexRSIValues ([Id], [Day], [Value], [IndexId])
SELECT nv.[Id], nv.[Day], nv.[Value], nv.[IndexId]
	FROM #rsivalues nv
	LEFT JOIN IndexRSIValues ixv ON ixv.IndexId = nv.IndexId AND ixv.[Day] = nv.[Day]
	WHERE nv.[Value] IS NOT NULL AND ixv.IndexId IS NULL

DROP TABLE #market;
DROP TABLE #rsivalues;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;





