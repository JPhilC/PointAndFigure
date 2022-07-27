/*
Missing Index Details from SQLQuery3.sql - DESKTOP-I4412V4\SQLEXPRESS.PnFData (DESKTOP-I4412V4\phil (60)) Executing...
The Query Processor estimates that implementing the following index could improve the query cost by 71.813%.
*/

/*
USE [PnFData]
GO
CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>]
ON [dbo].[EodPrices] ([Day])
INCLUDE ([ShareId],[AdjustedClose])
GO
*/
