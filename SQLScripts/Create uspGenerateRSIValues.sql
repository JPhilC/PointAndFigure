USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerateRSIValues]    Script Date: 08/03/2022 21:00:27 ******/
DROP PROCEDURE [dbo].[uspGenerateRSIValues]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerateRSIValues]    Script Date: 08/03/2022 21:00:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[uspGenerateRSIValues]
		@CutOffDate Date
	AS
SET NOCOUNT ON;

RAISERROR (N'Generating share v market RSI values ...', 0, 0) WITH NOWAIT;

-- Create relative to Market RSI Values
SELECT s.Id as ShareId, p.[Day], p.[AdjustedClose]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 0) AS [RelativeTo] 
	INTO #market
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeCode = s.ExchangeCode AND ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector IS NULL
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]

UPDATE sv
	SET sv.[Value] = nv.[Value]
FROM [dbo].[ShareRSIValues] sv
LEFT JOIN #market nv ON nv.[ShareId] = sv.ShareId AND sv.[RelativeTo]=nv.[RelativeTo] AND sv.[Day] = nv.[Day]
WHERE nv.[Value] IS NOT NULL AND nv.[Value] <> sv.[Value]

INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT nv.[ShareId], nv.[Day], nv.[Value], nv.[Id], nv.[RelativeTo] 
	FROM #market nv
	LEFT JOIN ShareRSIValues sv ON sv.[RelativeTo]=nv.[RelativeTo] AND sv.ShareId = nv.ShareId AND sv.[Day] = nv.[Day]
	WHERE nv.[Day] <= @CutOffDate AND sv.[Value] IS NULL AND nv.[Value] IS NOT NULL


-- Create relative Sector RSI Values
RAISERROR (N'Generating share v sector RSI values ...', 0, 0) WITH NOWAIT;

SELECT s.Id as ShareId, p.[Day], p.[AdjustedClose]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 1) AS [RelativeTo]
	INTO #sector
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeCode = s.ExchangeCode AND ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector = s.SuperSector
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]

UPDATE sv
	SET sv.[Value] = nv.[Value]
FROM [dbo].[ShareRSIValues] sv
LEFT JOIN #sector nv ON nv.[ShareId] = sv.ShareId AND sv.[RelativeTo] = nv.[RelativeTo] AND sv.[Day] = nv.[Day]
WHERE nv.[Value] IS NOT NULL AND nv.[Value] <> sv.[Value]

INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT nv.[ShareId], nv.[Day], nv.[Value], nv.[Id], nv.[RelativeTo] 
	FROM #sector nv
	LEFT JOIN ShareRSIValues sv ON sv.[RelativeTo]=nv.[RelativeTo] AND sv.ShareId = nv.ShareId AND sv.[Day] = nv.[Day]
	WHERE nv.[Day] <= @CutOffDate AND sv.[Value] IS NULL AND nv.[Value] IS NOT NULL



DROP TABLE #market
DROP TABLE #sector

RAISERROR (N'Done', 0, 0) WITH NOWAIT;

RETURN



GO


