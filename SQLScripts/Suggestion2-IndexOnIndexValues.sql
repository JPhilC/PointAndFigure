/*
Missing Index Details from SQLQuery3.sql - DESKTOP-I4412V4\SQLEXPRESS.PnFData (DESKTOP-I4412V4\phil (60))
The Query Processor estimates that implementing the following index could improve the query cost by 80.0536%.
*/


USE [PnFData]
GO
CREATE NONCLUSTERED INDEX [IX_IndexRSIValues_DayIncIndexIdValue]
ON [dbo].[IndexRSIValues] ([Day])
INCLUDE ([IndexId],[Value])
GO

