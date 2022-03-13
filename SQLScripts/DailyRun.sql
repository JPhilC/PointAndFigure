USE [PnFData]
GO

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

GO


