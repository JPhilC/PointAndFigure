USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspGetPortfolioEvents]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGetPortfolioEvents] 
AS
SET NOCOUNT ON

DECLARE @lastDay DATETIME
SELECT @lastDay =MAX([Day])
	FROM [ShareIndicators]

SELECT s.[Tidm]
	, s.[Name] [ShareName]
	, p.[Name] [Portfolio]
	, ps.[Holding]
	, q.[AdjustedClose]
	, si.[NewEvents]
	, ps.Remarks
	FROM PortfolioShares ps
	LEFT JOIN Portfolios p ON p.[Id] = ps.[PortfolioId]
	LEFT JOIN [Shares] s ON s.[Id] = ps.[ShareId]
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = ps.[ShareId] AND si.[Day] = @lastDay
	LEFT JOIN [EodPrices] q ON q.[ShareId] = ps.[ShareId] AND q.[Day] = @lastDay
	WHERE si.NewEvents IS NOT NULL AND si.NewEvents > 0
	ORDER BY [ShareName], [Portfolio]

GO

