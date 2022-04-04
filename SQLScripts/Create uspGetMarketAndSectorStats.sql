USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGetMarketAndSectorStats]    Script Date: 04/04/2022 11:53:37 ******/
DROP PROCEDURE [dbo].[uspGetMarketAndSectorStats]
GO

/****** Object:  StoredProcedure [dbo].[uspGetMarketAndSectorStats]    Script Date: 04/04/2022 11:53:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGetMarketAndSectorStats]
AS
SET NOCOUNT ON
DECLARE @day DATE 
SELECT @day = MAX([Day]) 
	FROM [IndexValues]

SELECT @day AS ResultsAsOf;

-- Market
SELECT ix.[ExchangeSubCode], ix.[SuperSector]
		, ii.[Rising]
		, ii.[RsRising]
		, ii.[Buy]
		, ii.[RsBuy]
		, ii.[BullishPercentRising]
		, ii.[PercentAbove10EmaRising]
		, ii.[PercentAbove30EmaRising]
		, ii.[PercentRSRisingRising]
		, ii.[PercentRSBuyRising]
		, ROUND(iv.[BullishPercent],1) AS [BullishPercent]
		, ROUND(iv.[PercentAboveEma10],1) AS [PercentAboveEma10]
		, ROUND(iv.[PercentAboveEma30],1) AS [PercentAboveEma30]
 		, ROUND(iv.[PercentRsRising],1) AS [PercentRsRising]
		, ROUND(iv.[PercentRsBuy],1) AS [PercentRsBuy]
		, ROUND(iv.[Contributors],1) AS [Contributors]
	FROM [Indices] ix
	LEFT JOIN [IndexValues] iv ON iv.[IndexId] = ix.[Id] AND iv.[Day] = @day
	LEFT JOIN [IndexIndicators] ii ON ii.[IndexId] = ix.[Id] AND ii.[Day] = @day
	WHERE ix.[SuperSector] IS NULL
    ORDER BY ix.[ExchangeSubCode]

-- Sectors
SELECT ix.[ExchangeSubCode], ix.[SuperSector]
		, ii.[Rising]
		, ii.[RsRising]
		, ii.[Buy]
		, ii.[RsBuy]
		, ii.[BullishPercentRising]
		, ii.[PercentAbove10EmaRising]
		, ii.[PercentAbove30EmaRising]
		, ii.[PercentRSRisingRising]
		, ii.[PercentRSBuyRising]
		, CONVERT(INT, ii.[Rising])
		+ CONVERT(INT, ii.[RsRising])
		+ CONVERT(INT, ii.[Buy])
		+ CONVERT(INT, ii.[RsBuy])
		+ CONVERT(INT, ii.[BullishPercentRising])
		+ CONVERT(INT, ii.[PercentAbove10EmaRising])
		+ CONVERT(INT, ii.[PercentAbove30EmaRising])
		+ CONVERT(INT, ii.[PercentRSRisingRising])
		+ CONVERT(INT, ii.[PercentRSBuyRising]) AS [Score]
		, ROUND(iv.[BullishPercent],1) AS [BullishPercent]
		, ROUND(iv.[PercentAboveEma10],1) AS [PercentAboveEma10]
		, ROUND(iv.[PercentAboveEma30],1) AS [PercentAboveEma30]
 		, ROUND(iv.[PercentRsRising],1) AS [PercentRsRising]
		, ROUND(iv.[PercentRsBuy],1) AS [PercentRsBuy]
		, ROUND(iv.[Contributors],1) AS [Contributors]
	FROM [Indices] ix
	LEFT JOIN [IndexValues] iv ON iv.[IndexId] = ix.[Id] AND iv.[Day] = @day
	LEFT JOIN [IndexIndicators] ii ON ii.[IndexId] = ix.[Id] AND ii.[Day] = @day
	WHERE ix.[SuperSector] IS NOT NULL
    ORDER BY ix.[ExchangeSubCode], [Score] DESC



--SELECT s.[Tidm], s.[Name]
--	, si.[Rising]
--	, si.[DoubleTop]
--	, si.[Ema10]
--	, si.[ClosedAboveEma10]
--	, si.[Ema30]
--	, si.[ClosedAboveEma30]
--	, si.[RsRising]
--	, si.[RsBuy]
--	, si.[PeerRsRising]
--	, si.[PeerRsBuy]
--	, si.[PeerRsFalling]
--FROM [Shares] s
--LEFT JOIN [ShareIndicators] si ON si.[ShareId] = s.[Id] AND si.[Day]='2022-04-01'
--WHERE s.[ExchangeSubCode] = 'AIM' AND s.[SuperSector] = 'Automobiles and Parts'

RETURN

GO


