USE [PnFData]
GO
IF OBJECT_ID('dbo.[uspUpdateSIBIndicators]', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspUpdateSIBIndicators;  
GO  

/****** Object:  StoredProcedure [dbo].[uspGenerateMarketIndices]    Script Date: 05/03/2022 21:30:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE uspUpdateSIBIndicators
	@CutOffDate DATE
AS
SET NOCOUNT ON
RAISERROR (N'Start of post initial chart creation run  ...', 0, 0) WITH NOWAIT;
DECLARE @SIResult int
DECLARE @IIResult int
DECLARE @BIResult int
DECLARE @Result int

EXECUTE @SIResult = [dbo].[uspUpdateShareIndicators] @CutOffDate

EXECUTE @IIResult = [dbo].[uspUpdateIndexIndicators] 

EXECUTE @BIResult = [dbo].[uspUpdateBullishIndicators] 

IF @SIREsult=0 AND @IIResult = 0 AND @BIResult = 0
BEGIN
	RAISERROR (N'Run complete.', 0, 0) WITH NOWAIT;
	SET @Result = 0
END
ELSE
BEGIN
	RAISERROR (N'There was an error running the Share, Index and Bullish percent updates.', 0, 0) WITH NOWAIT;
	SET @Result = 1
END
RETURN @Result

GO


