USE [PnFData]
GO

DECLARE @RC int
DECLARE @upToDate datetime

SET @upToDate = getdate()

EXECUTE @RC = [dbo].[uspGenerateMarketIndices] 
   @upToDate

EXECUTE @RC = [dbo].[uspGenerateSectorIndices] 
   @upToDate
GO


