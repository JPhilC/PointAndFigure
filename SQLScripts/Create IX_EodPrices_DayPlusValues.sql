/*
Missing Index Details from SQLQuery3.sql - DESKTOP-I4412V4\SQLEXPRESS.PnFData (DESKTOP-I4412V4\phil (60))
The Query Processor estimates that implementing the following index could improve the query cost by 76.5459%.
*/


USE [PnFData]
GO
CREATE NONCLUSTERED INDEX [IX_EodPrices_DayPlusValues]
ON [dbo].[EodPrices] ([Day])
INCLUDE ([ShareId],[AdjustedClose],[New52WeekHigh],[New52WeekLow])
GO

