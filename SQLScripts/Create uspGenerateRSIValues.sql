USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerateMarketIndices]    Script Date: 05/03/2022 21:30:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGenerateRSIValues]
	AS
SET NOCOUNT ON;

-- Create relative to Market RSI Values
INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT s.Id as ShareId, p.[Day], p.[Close]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 0) AS [RelativeTo] 
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector = s.SuperSector
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]
	LEFT JOIN ShareRSIValues rsi ON rsi.[RelativeTo]=0 AND rsi.ShareId = s.Id AND rsi.[Day] = p.[Day]
	WHERE rsi.Value IS NULL AND ixv.Value IS NOT NULL

-- Create relative Sector RSI Values
INSERT INTO ShareRSIValues ([ShareId], [Day], [Value], [Id], [RelativeTo])
SELECT s.Id as ShareId, p.[Day], p.[Close]/ixv.[Value] * 1000 as [Value], NEWID() as Id, CONVERT(int, 1) AS [RelativeTo]
	FROM EodPrices p
	LEFT JOIN Shares s ON p.ShareId = s.Id
	LEFT JOIN Indices ix ON ix.ExchangeSubCode = s.ExchangeSubCode AND ix.SuperSector = s.SuperSector
	LEFT JOIN IndexValues ixv ON ixv.IndexId = ix.Id AND ixv.[Day] = p.[Day]
	LEFT JOIN ShareRSIValues rsi ON rsi.[RelativeTo]=1 AND rsi.ShareId = s.Id AND rsi.[Day] = p.[Day]
	WHERE rsi.Value IS NULL AND ixv.Value IS NOT NULL

RETURN



