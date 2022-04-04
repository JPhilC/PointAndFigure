/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [Id]
      ,[Version]
      ,[CreatedAt]
      ,[UpdatedAt]
      ,[ExchangeCode]
      ,[ExchangeSubCode]
      ,[SuperSector]
  FROM [PnFData].[dbo].[Indices]

--SELECT i.[ExchangeSubCode], i.[SuperSector], p.[Day] 
--		, count(*) [Members]
--	FROM [PnFData].[dbo].[Indices] i
--	RIGHT OUTER JOIN [PnFData].[dbo].[Shares] s on s.ExchangeSubCode = i.ExchangeSubCode and s.SuperSector = i.SuperSector
--	RIGHT OUTER JOIN [PnFData].[dbo].[EodPrices] p on p.ShareId = s.Id
--	GROUP BY i.ExchangeSubCode, i.SuperSector, p.[Day]
--	ORDER BY i.ExchangeSubCode, i.SuperSector, p.[Day] DESC






SELECT i.[ExchangeSubCode], i.[SuperSector], si.[Day] 
		, count(*) [Members]
		, CONVERT(FLOAT, SUM(IIF(si.[Rising]=1,1,0))) [Rising] 
		, CONVERT(FLOAT,SUM(COALESCE(si.[ClosedAboveEma10], 0))) [Percent10]
		, CONVERT(FLOAT, SUM(COALESCE(si.[ClosedAboveEma30], 0))) [Percent30]
    INTO #Sectors
	FROM [PnFData].[dbo].[Indices] i
	RIGHT OUTER JOIN [PnFData].[dbo].[Shares] s on s.ExchangeSubCode = i.ExchangeSubCode and s.SuperSector = i.SuperSector
	RIGHT OUTER JOIN [PnFData].[dbo].[ShareIndicators] si on si.ShareId = s.Id
    WHERE i.[SuperSector] IS NOT NULL
	GROUP BY i.ExchangeSubCode, i.SuperSector, si.[Day]
	ORDER BY i.ExchangeSubCode, i.SuperSector, si.[Day] DESC

SELECT i.[ExchangeSubCode], i.[SuperSector], i.[Day]
		, i.[Rising]/i.[Members]*100 [BullishPercent]
		, i.[Percent10]/i.[Members]*100 [Percent10]
		, i.[Percent30]/i.[Members]*100 [Percent30]
	FROM #Sectors i
	ORDER BY i.ExchangeSubCode, i.SuperSector, i.[Day] DESC

DROP TABLE #Sectors
