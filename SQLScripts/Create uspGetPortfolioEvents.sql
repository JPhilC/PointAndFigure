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

DECLARE @NewDoubleTop AS INT			= 0x00001;
DECLARE @NewTripleTop AS INT			= 0x00002;
DECLARE @NewRsBuy AS INT				= 0x00004;
DECLARE @NewPeerRsBuy AS INT			= 0x00008;
DECLARE @NewDoubleBottom AS INT			= 0x00010;
DECLARE @NewTripleBottom AS INT			= 0x00020;
DECLARE @NewRsSell AS INT				= 0x00040;
DECLARE @NewPeerRsSell AS INT			= 0x00080;
DECLARE @NewCloseAboveEma10 AS INT		= 0x00100;
DECLARE @NewCloseAboveEma30 AS INT		= 0x00200;
DECLARE @NewDropBelowEma10 AS INT		= 0x00400;
DECLARE @NewDropBelowEma30 AS INT		= 0x00800;
DECLARE @NewBullSupportBreach AS INT	= 0x01000;
DECLARE @High52Week AS INT				= 0x02000;
DECLARE @Low52Week AS INT				= 0x04000;
DECLARE @MomentumGonePositive AS INT    = 0x08000;
DECLARE @MomentumGoneNegative AS INT	= 0x10000;
DECLARE @SwitchedToRising AS INT		= 0x20000;
DECLARE @SwitchedToFalling AS INT		= 0x40000;


SELECT s.[Tidm]
	, s.[Name] [ShareName]
	, p.[Name] [Portfolio]
	, si.[NewEvents]
	, IIF(si.[NewEvents]&@SwitchedToRising=@SwitchedToRising,1,0) [Rising]
	, IIF(si.[NewEvents]&@SwitchedToFalling=@SwitchedToFalling,1,0) [Falling]
	, IIF(si.[NewEvents]&@NewDoubleTop=@NewDoubleTop,1,0) [Buy]
	, IIF(si.[NewEvents]&@NewTripleTop=@NewTripleTop,1,0) [BuyBuy]
	, IIF(si.[NewEvents]&@NewDoubleBottom=@NewDoubleBottom,1,0) [Sell]
	, IIF(si.[NewEvents]&@NewTripleBottom=@NewTripleBottom,1,0) [SellSell]
	, IIF(si.[NewEvents]&@NewBullSupportBreach=@NewBullSupportBreach,1,0) [SupportBreach]
	, IIF(si.[NewEvents]&@NewRsBuy=@NewRsBuy,1,0) [RsBuy]
	, IIF(si.[NewEvents]&@NewRsSell=@NewRsSell,1,0) [RsSell]
	, IIF(si.[NewEvents]&@NewPeerRsBuy=@NewPeerRsBuy,1,0) [PeerRsBuy]
	, IIF(si.[NewEvents]&@NewPeerRsSell=@NewPeerRsSell,1,0) [PeerRsSell]
	, IIF(si.[NewEvents]&@NewCloseAboveEma10=@NewCloseAboveEma10,1,0) [AboveEma10]
	, IIF(si.[NewEvents]&@NewDropBelowEma10=@NewDropBelowEma10,1,0) [BelowEma10]
	, IIF(si.[NewEvents]&@NewCloseAboveEma30=@NewCloseAboveEma30,1,0) [AboveEma30]
	, IIF(si.[NewEvents]&@NewDropBelowEma30=@NewDropBelowEma30,1,0) [BelowEma30]
	, IIF(si.[NewEvents]&@High52Week=@High52Week,1,0) [High52W]
	, IIF(si.[NewEvents]&@Low52Week=@Low52Week,1,0) [Low52W]
	, IIF(si.[NewEvents]&@MomentumGonePositive=@MomentumGonePositive,1,0) [MomentumUp]
	, IIF(si.[NewEvents]&@MomentumGoneNegative=@MomentumGoneNegative,1,0) [MomentumDown]
	FROM PortfolioShares ps
	LEFT JOIN Portfolios p ON p.[Id] = ps.[PortfolioId]
	LEFT JOIN [Shares] s ON s.[Id] = ps.[ShareId]
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = ps.[ShareId] AND si.[Day] = @lastDay
	WHERE si.NewEvents IS NOT NULL AND si.NewEvents > 0
	ORDER BY [ShareName], [Portfolio]

GO

