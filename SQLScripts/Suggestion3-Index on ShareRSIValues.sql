/*
Missing Index Details from SQLQuery3.sql - DESKTOP-I4412V4\SQLEXPRESS.PnFData (DESKTOP-I4412V4\phil (60)) Executing...
The Query Processor estimates that implementing the following index could improve the query cost by 21.1351%.
*/


USE [PnFData]
GO
CREATE NONCLUSTERED INDEX [IX_ShareRSIValues_RelToDayShareIdIncValue]
ON [dbo].[ShareRSIValues] ([RelativeTo],[Day],[ShareId])
INCLUDE ([Value])
GO

