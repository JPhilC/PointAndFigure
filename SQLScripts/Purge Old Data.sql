
drop table #SortedCharts
go

SELECT  sc.[ShareId]
	,   sc.[ChartId]
	,   c.[Source]
	,	c.[BoxSize]
	,   sc.[CreatedAt]
	,	ROW_NUMBER() OVER(PARTITION BY sc.[ShareId], c.[Source] ORDER BY sc.[CreatedAt] DESC) as Ordinal# 
	INTO #SortedCharts
	FROM [ShareCharts] sc
	LEFT JOIN [PnFCharts] c ON c.[Id] = sc.[ChartId]


DELETE FROM [PnFCharts]
	WHERE [Id] IN (SELECT [ChartId]	FROM #SortedCharts	WHERE [Ordinal#] >2 )


DROP TABLE #SortedCharts;
go

SELECT  sc.[IndexId]
	,   sc.[ChartId]
	,   c.[Source]
	,	c.[BoxSize]
	,   sc.[CreatedAt]
	,	ROW_NUMBER() OVER(PARTITION BY sc.[IndexId], c.[Source] ORDER BY sc.[CreatedAt] DESC) as Ordinal# 
	INTO #SortedCharts
	FROM [IndexCharts] sc
	LEFT JOIN [PnFCharts] c ON c.[Id] = sc.[ChartId]

SELECT * FROM #SortedCharts

delete from EodPrices
	where [Day] < '2019-01-01'


