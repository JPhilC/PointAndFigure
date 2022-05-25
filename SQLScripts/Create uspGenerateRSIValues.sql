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

RAISERROR (N'Generating share RSI values ...', 0, 0) WITH NOWAIT;

-- Create relative to Market RSI Values
INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT s.Id as ShareId, p.[Day], p.[AdjustedClose]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 0) AS [RelativeTo] 
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeCode = s.ExchangeCode AND ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector IS NULL
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]
	LEFT JOIN ShareRSIValues rsi ON rsi.[RelativeTo]=0 AND rsi.ShareId = s.Id AND rsi.[Day] = p.[Day]
	WHERE p.[Day] <= @CutOffDate AND rsi.Value IS NULL AND ixv.Value IS NOT NULL

-- Create relative Sector RSI Values
INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT s.Id as ShareId, p.[Day], p.[AdjustedClose]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 1) AS [RelativeTo]
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeCode = s.ExchangeCode AND ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector = s.SuperSector
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]
	LEFT JOIN ShareRSIValues rsi ON rsi.[RelativeTo]=1 AND rsi.ShareId = s.Id AND rsi.[Day] = p.[Day]
	WHERE p.[Day] <= @CutOffDate AND rsi.Value IS NULL AND ixv.Value IS NOT NULL

RAISERROR (N'Done', 0, 0) WITH NOWAIT;

RETURN



GO


