USE [PnFData]
GO

RAISERROR (N'Start of post initial chart creation run  ...', 0, 0) WITH NOWAIT;
DECLARE @RC int

EXECUTE @RC = [dbo].[uspUpdateShareIndicators] 

EXECUTE @RC = [dbo].[uspUpdateIndexIndicators] 

EXECUTE @RC = [dbo].[uspUpdateBullishIndicators] 

RAISERROR (N'Run complete.', 0, 0) WITH NOWAIT;

GO


