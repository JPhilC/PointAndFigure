IF object_id('tempdb..#LatestCharts','U') is not null
	DROP TABLE #LatestCharts;

IF object_id('tempdb..#SortedByIndex','U') is not null
	DROP TABLE #SortedByIndex;

IF object_id('tempdb..#rising','U') is not null
	DROP TABLE #rising;

IF object_id('tempdb..#doubleTop','U') is not null
	DROP TABLE #doubleTop;

IF object_id('tempdb..#tripleTop','U') is not null
	DROP TABLE #tripleTop;

	IF object_id('tempdb..#falling','U') is not null
	DROP TABLE #falling;

IF object_id('tempdb..#doubleBottom','U') is not null
	DROP TABLE #doubleBottom;

IF object_id('tempdb..#tripleBottom','U') is not null
	DROP TABLE #tripleBottom;


-- Get the latest charts for each type
-- Chart sources
--		0 - Stock
--		2 - Stock RS
--		3 - Peer RS
SELECT sc.[ShareId]
	, c.[Source]
	, ROW_NUMBER() OVER(PARTITION BY sc.[ShareId], c.[Source] ORDER BY c.GeneratedDate DESC) as Ordinal# 
	, sc.[ChartId]
	, c.[GeneratedDate]
	, c.[CreatedAt]
	INTO #LatestCharts
	FROM ShareCharts sc
	LEFT JOIN PnFCharts c on c.Id = sc.ChartId
	WHERE c.[Source] IN (0, 2, 3)


SELECT col.[Id]
      ,col.[PnFChartId]
      ,col.[ColumnType]
      ,col.[CurrentBoxIndex]
      ,col.[Index]
	  ,ROW_NUMBER() OVER(PARTITION BY [PnfChartId] ORDER BY [Index] DESC) AS Ordinal#
	  ,c.[Source]
	  ,sc.[ShareId]
  INTO #SortedByIndex
  FROM [PnFColumns] col
  LEFT JOIN [PnFCharts] c ON c.Id = col.PnFChartId
  LEFT JOIN [ShareCharts] sc ON sc.ChartId = col.PnFChartId
  WHERE col.[PnFChartId] IN (SELECT ChartId FROM #LatestCharts WHERE Ordinal# = 1) 
  ORDER BY [PnfChartId], [Index] DESC

-- The Bulls
SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source], [CurrentBoxIndex] AS H1
	INTO #rising
	FROM #SortedByIndex sbi
	WHERE sbi.[Ordinal#] = 1 AND sbi.[ColumnType]=1;

SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source], sbi.[CurrentBoxIndex] AS H3, c1.H1  
	INTO #doubleTop
	FROM #SortedByIndex AS sbi
	JOIN #rising AS c1 ON c1.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c1.H1;


SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source]  
	INTO #TripleTop
	FROM #SortedByIndex AS sbi
	JOIN #doubleTop AS c3 ON c3.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 5 AND sbi.[ColumnType]=1
		AND sbi.[CurrentBoxIndex] < c3.H3;

-- The Bears
SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source], [CurrentBoxIndex] AS L1
	INTO #falling
	FROM #SortedByIndex sbi
	WHERE sbi.[Ordinal#] = 1 AND sbi.[ColumnType]=0;

SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source], sbi.[CurrentBoxIndex] AS L3, f.L1  
	INTO #doubleBottom
	FROM #SortedByIndex AS sbi
	JOIN #falling AS f ON f.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 3 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > f.L1;


SELECT sbi.[PnfChartId], sbi.[ShareId], sbi.[Source]  
	INTO #TripleBottom
	FROM #SortedByIndex AS sbi
	JOIN #doubleBottom AS db ON db.[PnFChartId] = sbi.[PnFChartId]
	WHERE sbi.[Ordinal#] = 5 AND sbi.[ColumnType]=0
		AND sbi.[CurrentBoxIndex] > db.L3;


select s.[Id] as [ShareId]
	, s.Tidm
	, s.[Name]
	, CONVERT(DATE, GETDATE()) as [Day]
	, CONVERT(bit, IIF(sh.[ShareId] IS NOT NULL, 1, 0)) AS Rising
	, CONVERT(bit, IIF(shdt.[ShareId] IS NOT NULL, 1, 0)) AS DoubleTop
	, CONVERT(bit, IIF(shtt.[ShareId] IS NOT NULL, 1, 0)) AS TripleTop
	, CONVERT(bit, IIF(srs.[ShareId] IS NOT NULL, 1, 0)) AS ShareRsRising
	, CONVERT(bit, IIF(srsBuy.[ShareId] IS NOT NULL, 1, 0)) AS ShareRsBuy
	, CONVERT(bit, IIF(prs.[ShareId] IS NOT NULL, 1, 0)) AS PeerRsRising
	, CONVERT(bit, IIF(prsBuy.[ShareId] IS NOT NULL, 1, 0)) AS PeerRsBuy
	, CONVERT(bit, IIF(f.[ShareId] IS NOT NULL, 1, 0)) AS Falling
	, CONVERT(bit, IIF(fdb.[ShareId] IS NOT NULL, 1, 0)) AS DoubleBottom
	, CONVERT(bit, IIF(ftb.[ShareId] IS NOT NULL, 1, 0)) AS TripleBottom
	, CONVERT(bit, IIF(srsf.[ShareId] IS NOT NULL, 1, 0)) AS ShareRsFalling
	, CONVERT(bit, IIF(srsSell.[ShareId] IS NOT NULL, 1, 0)) AS ShareRsSell
	, CONVERT(bit, IIF(prsf.[ShareId] IS NOT NULL, 1, 0)) AS PeerRsFalling
	, CONVERT(bit, IIF(prsSell.[ShareId] IS NOT NULL, 1, 0)) AS PeerRsSell
from [Shares] s
left join #rising sh ON sh.[ShareId] = s.Id AND sh.[Source] = 0
left join #doubleTop shdt ON shdt.[ShareId] = s.Id AND shdt.[Source] = 0
left join #tripleTop shtt ON shtt.[ShareId] = s.Id AND shtt.[Source] = 0
left join #rising srs ON srs.[ShareId] = s.Id AND srs.[Source] = 2
left join #doubleTop srsBuy ON srsBuy.[ShareId] = s.Id AND srsBuy.[Source] = 2
left join #rising prs ON prs.[ShareId] = s.Id AND prs.[Source] = 3
left join #doubleTop prsBuy ON prsBuy.[ShareId] = s.Id AND prsBuy.[Source] = 3
left join #falling f ON f.[ShareId] = s.Id AND f.[Source] = 0
left join #doubleBottom fdb ON fdb.[ShareId] = s.Id AND fdb.[Source] = 0
left join #tripleBottom ftb ON ftb.[ShareId] = s.Id AND ftb.[Source] = 0
left join #falling srsf ON srsf.[ShareId] = s.Id AND srsf.[Source] = 2
left join #doubleBottom srsSell ON srsSell.[ShareId] = s.Id AND srsSell.[Source] = 2
left join #falling prsf ON prsf.[ShareId] = s.Id AND prsf.[Source] = 3
left join #doublebottom prsSell ON prsSell.[ShareId] = s.Id AND prsSell.[Source] = 3

