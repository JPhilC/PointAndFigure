USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGetSectorShareStats]    Script Date: 04/04/2022 11:51:36 ******/
DROP PROCEDURE [dbo].[uspGetSectorShareStats]
GO

/****** Object:  StoredProcedure [dbo].[uspGetSectorShareStats]    Script Date: 04/04/2022 11:51:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGetSectorShareStats] 
( @ExchangeCode NVARCHAR(10)
, @ExchangeSubCode NVARCHAR(4)
, @SuperSector NVARCHAR(50)
)
AS
SET NOCOUNT ON
DECLARE @day DATE 
SELECT @day = MAX([Day]) 
	FROM [EodPrices]

SELECT @day AS ResultsAsOf;

SELECT s.[Tidm], s.[Name]
	, p.[AdjustedClose]
	, si.[Rising]
	, si.[DoubleTop]
	, si.[TripleTop]
	, si.[RsRising]
	, si.[RsBuy]
	, si.[PeerRsRising]
	, si.[PeerRsBuy]
	, si.[ClosedAboveEma10]
	, si.[ClosedAboveEma30]
	, CONVERT(INT, si.[Rising])
	+ CONVERT(INT, si.[DoubleTop])
	+ CONVERT(INT, si.[TripleTop])
	+ CONVERT(INT, si.[RsRising])
	+ CONVERT(INT, si.[RsBuy])
	+ CONVERT(INT, si.[PeerRsRising])
	+ CONVERT(INT, si.[PeerRsBuy])
	+ CONVERT(INT, si.[ClosedAboveEma10])
	+ CONVERT(INT, si.[ClosedAboveEma30]) AS [Score]
	, si.[Ema10]
	, si.[Ema30]
FROM [Shares] s
LEFT JOIN [EodPrices] p ON p.[ShareId] = s.[Id] AND p.[Day] = @day 
LEFT JOIN [ShareIndicators] si ON si.[ShareId] = s.[Id] AND si.[Day]=@day
WHERE s.[ExchangeCode] = @ExchangeCode 
	AND s.[ExchangeSubCode] = @ExchangeSubCode 
	AND s.[SuperSector] = @SuperSector
ORDER BY [Score] DESC

RETURN

GO


