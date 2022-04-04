USE [PnFData]
GO

RAISERROR (N'Start of daily run  ...', 0, 0) WITH NOWAIT;
DECLARE @RC int
DECLARE @upToDate datetime
SET @uptoDate = GETDATE()
-- TODO: Set parameter values here.


EXECUTE @RC = [dbo].[uspGenerateMarketIndices] 
   @upToDate

EXECUTE @RC = [dbo].[uspGenerateSectorIndices] 
   @upToDate

EXECUTE @RC = [dbo].[uspGenerateIndexRSIValues] 

EXECUTE @RC = [dbo].[uspGenerateRSIValues] 

EXECUTE @RC = [dbo].[uspGenerate10And30WeekEmas]

RAISERROR (N'Daily run complete.', 0, 0) WITH NOWAIT;

GO


