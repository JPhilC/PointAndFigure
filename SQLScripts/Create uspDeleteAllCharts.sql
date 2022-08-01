USE [PnFData]
GO
IF OBJECT_ID('dbo.[uspDeleteAllCharts]', 'P') IS NOT NULL  
   DROP PROCEDURE dbo.uspDeleteAllCharts;  
GO  

/****** Object:  StoredProcedure [dbo].[uspDeleteAllCharts]    Script Date: 05/03/2022 21:30:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE uspDeleteAllCharts
	
AS
SET NOCOUNT ON
RAISERROR (N'Deleting all P & F Charts  ...', 0, 0) WITH NOWAIT;

TRUNCATE TABLE [ShareCharts]
TRUNCATE TABLE [IndexCharts]
TRUNCATE TABLE [PnFSignals]
TRUNCATE TABLE [PnFBoxes]
DELETE FROM [PnFColumns]
DELETE FROM [PnFCharts]


GO


