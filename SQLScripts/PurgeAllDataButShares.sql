USE [PnFData]
GO

-- truncate table EodPrices
truncate table ShareCharts
truncate table ShareIndicators
truncate table ShareRSIValues

truncate table IndexCharts
truncate table IndexIndicators
truncate table IndexRSIValues
truncate table IndexValues
DELETE FROM Indices

truncate table PnFSignals
truncate table PnFBoxes
delete from PnFColumns
delete from PnFCharts
