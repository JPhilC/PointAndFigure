IF object_id('tempdb..#SortedByIndex','U') is not null
	DROP TABLE #SortedByIndex;

IF object_id('tempdb..#column1','U') is not null
	DROP TABLE #column1;

IF object_id('tempdb..#column3','U') is not null
	DROP TABLE #column3;

IF object_id('tempdb..#column5','U') is not null
	DROP TABLE #column5;

IF object_id('tempdb..#column7','U') is not null
	DROP TABLE #column7;

SELECT [Id]
      ,[PnFChartId]
      ,[ColumnType]
      ,[CurrentBoxIndex]
      ,[Index]
	  ,ROW_NUMBER() OVER(PARTITION BY [PnfChartId] ORDER BY [Index] DESC) AS Ordinal#
  INTO #SortedByIndex
  FROM [PnFData].[dbo].[PnFColumns]
  ORDER BY [PnfChartId], [Index] DESC

SELECT [PnfChartId], [CurrentBoxIndex] AS L1
	INTO #column1
	FROM #SortedByIndex
	WHERE [Ordinal#] = 1 AND [ColumnType]=0;

SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS L3, c1.L1  
	INTO #column3
	FROM #SortedByIndex AS sbi
	JOIN #column1 AS c1 ON c1.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > c1.L1;


SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS L5, c3.L3, c3.L1  
	INTO #column5
	FROM #SortedByIndex AS sbi
	JOIN #column3 AS c3 ON c3.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 5 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > c3.L3;

SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS L7, c5.L5, c5.L3, c5.L1  
	INTO #column7
	FROM #SortedByIndex AS sbi
	JOIN #column5 AS c5 ON c5.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 7 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > c5.L5;


-- Double bottom
select s.Tidm, ch.BoxSize, ch.GeneratedDate as [Date], c.L5, c.L3, c.L1
	from #column5 as c
	LEFT JOIN [ShareCharts] sc ON sc.ChartId = c.[PnFChartId]
	LEFT JOIN [PnFCharts] ch ON ch.Id = sc.[ChartId]
	LEFT JOIN [Shares] s ON s.Id = sc.ShareId;

-- Triple bottom
select s.Tidm, ch.BoxSize, ch.GeneratedDate AS [Date], c.L7, c.L5, c.L3, c.L1
	from #column7 as c
	LEFT JOIN [ShareCharts] sc ON sc.ChartId = c.[PnFChartId]
	LEFT JOIN [PnFCharts] ch ON ch.Id = sc.[ChartId]
	LEFT JOIN [Shares] s ON s.Id = sc.ShareId;

