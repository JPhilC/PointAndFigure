USE [PnFData]
GO

truncate table EodPrices
truncate table ShareCharts
truncate table ShareRSIValues

truncate table IndexCharts
truncate table IndexIndicators
truncate table IndexRSIValues
truncate table IndexValues
DELETE FROM Indices

truncate table PnFSignals
truncate table PnFBoxes
DELETE FROM PnFColumns
delete from PnFCharts
