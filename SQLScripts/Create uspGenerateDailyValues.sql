USE [PnFData]
GO

IF OBJECT_ID('dbo.[uspGenerateDailyValues]', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspGenerateDailyValues;  
GO  

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE uspGenerateDailyValues 
	@CutOffDate DATE
AS
SET NOCOUNT ON
RAISERROR (N'Start of daily run  ...', 0, 0) WITH NOWAIT;
DECLARE @RC int

EXECUTE @RC = [dbo].[uspGenerate52WeekHighsAndLows]

IF @RC = 0
  EXECUTE @RC = [dbo].[uspGenerateMarketIndices] @CutOffDate

IF @RC = 0
 EXECUTE @RC = [dbo].[uspGenerateSectorIndices] @CutOffDate 

IF @RC = 0
 EXECUTE @RC = [dbo].[uspGenerateIndexRSIValues] @CutOffDate

IF @RC = 0
 EXECUTE @RC = [dbo].[uspGenerateRSIValues] @CutOffDate


IF @RC = 0
 EXECUTE @RC = [dbo].[uspGenerate10And30WeekEmas]

IF @RC = 0
 EXECUTE @RC =  [dbo].[uspGenerate10WeekHighLowEma]

IF @RC = 0
	BEGIN
		RAISERROR (N'Daily run complete.', 0, 0) WITH NOWAIT;
		RETURN 0
	END
ELSE
	BEGIN
		RAISERROR (N'There was an error in the daily run.', 0, 0) WITH NOWAIT;
		RETURN 1
	END

GO


