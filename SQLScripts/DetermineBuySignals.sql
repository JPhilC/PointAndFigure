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

SELECT [PnfChartId], [CurrentBoxIndex] AS H1
	INTO #column1
	FROM #SortedByIndex
	WHERE [Ordinal#] = 1 AND [ColumnType]=1;

SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS H3, c1.H1  
	INTO #column3
	FROM #SortedByIndex AS sbi
	JOIN #column1 AS c1 ON c1.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c1.H1;


SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS H5, c3.H3, c3.H1  
	INTO #column5
	FROM #SortedByIndex AS sbi
	JOIN #column3 AS c3 ON c3.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 5 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c3.H3;

SELECT sbi.[PnfChartId], sbi.[CurrentBoxIndex] AS H7, c5.H5, c5.H3, c5.H1  
	INTO #column7
	FROM #SortedByIndex AS sbi
	JOIN #column5 AS c5 ON c5.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 7 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c5.H5;


-- Double top
select s.Tidm, ch.BoxSize, ch.GeneratedDate as [Date], c.H5, c.H3, c.H1
	from #column5 as c
	LEFT JOIN [ShareCharts] sc ON sc.ChartId = c.[PnFChartId]
	LEFT JOIN [PnFCharts] ch ON ch.Id = sc.[ChartId]
	LEFT JOIN [Shares] s ON s.Id = sc.ShareId;

-- Triple top
select s.Tidm, ch.BoxSize, ch.GeneratedDate AS [Date], c.H7, c.H5, c.H3, c.H1
	from #column7 as c
	LEFT JOIN [ShareCharts] sc ON sc.ChartId = c.[PnFChartId]
	LEFT JOIN [PnFCharts] ch ON ch.Id = sc.[ChartId]
	LEFT JOIN [Shares] s ON s.Id = sc.ShareId;

