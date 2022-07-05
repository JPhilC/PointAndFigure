USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerateSectorIndices]    Script Date: 13/03/2022 17:38:53 ******/
DROP PROCEDURE [dbo].[uspGenerate10And30WeekEmas]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerate10And30WeekEmas]    Script Date: 13/03/2022 17:38:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGenerate10And30WeekEmas] 
	AS
SET NOCOUNT ON

RAISERROR (N'Generating 1, 5, 10 and 30 week share value EMAs ...', 0, 0) WITH NOWAIT;

IF OBJECT_ID('tempdb..#TBL_EMA_RT') IS NOT NULL BEGIN
    DROP TABLE #TBL_EMA_RT
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_1') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_1
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_5') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_5
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_10') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_10
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_30') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_30
END

-- Generate 10 week EMA (50 day periods)
DECLARE @Periods1 int, @Periods5 int, @Periods10 int, @Periods30 int, @K1 FLOAT, @K5 FLOAT, @K10 FLOAT, @K30 FLOAT
DECLARE @Ema1 FLOAT, @Ema5 FLOAT, @Ema10 FLOAT, @Ema30 FLOAT
SET @Periods1 = 5;
SET @Periods5 = 25;
SET @Periods10 = 50;
SET @Periods30 = 150;
SET @K1 = 2.0/(1+@Periods1);
SET @K5 = 2.0/(1+@Periods5);
SET @K10 = 2.0/(1+@Periods10);
SET @K30 = 2.0/(1+@Periods30);
 
SELECT [ShareId] 
	,	ROW_NUMBER() OVER (PARTITION BY [ShareId] ORDER BY [ShareId],[Day]) AS [Day#]
	,	[Day]
	,	[AdjustedClose]
	,	CAST(NULL AS FLOAT) AS StartingAverage1
	,	CAST(NULL AS FLOAT) AS StartingAverage5
	,	CAST(NULL AS FLOAT) AS StartingAverage10
	,	CAST(NULL AS FLOAT) AS StartingAverage30
	,	CAST(NULL AS FLOAT) AS Ema1
	,	CAST(NULL AS FLOAT) AS Ema5
	,	CAST(NULL AS FLOAT) AS Ema10
	,	CAST(NULL AS FLOAT) AS Ema30
INTO #TBL_EMA_RT
FROM [EodPrices]
WHERE [AdjustedClose] < 1000000.00		
ORDER BY [ShareId], [Day#]

--- The large value comparison is to filter out rubbish data coming from AlphaVantage

 
CREATE UNIQUE CLUSTERED INDEX EMA_IDX_RT ON #TBL_EMA_RT ([ShareId], [Day#])

SELECT [ShareId], AVG([AdjustedClose]) AS StartAvg 
	INTO #TBL_START_AVG_1
	FROM #TBL_EMA_RT 
	WHERE [Day#] <= @Periods1 
	GROUP BY [ShareId]
 
SELECT [ShareId], AVG([AdjustedClose]) AS StartAvg 
	INTO #TBL_START_AVG_5
	FROM #TBL_EMA_RT 
	WHERE [Day#] <= @Periods5 
	GROUP BY [ShareId]

SELECT [ShareId], AVG([AdjustedClose]) AS StartAvg 
	INTO #TBL_START_AVG_10
	FROM #TBL_EMA_RT 
	WHERE [Day#] <= @Periods10 
	GROUP BY [ShareId]
 
SELECT [ShareId], AVG([AdjustedClose]) AS StartAvg 
	INTO #TBL_START_AVG_30
	FROM #TBL_EMA_RT 
	WHERE [Day#] <= @Periods30 
	GROUP BY [ShareId]

UPDATE #TBL_EMA_RT
	SET [StartingAverage1] = t1.[StartAvg]
	,	[StartingAverage5] = t5.[StartAvg]
	,	[StartingAverage10] = t10.[StartAvg]
	,	[StartingAverage30] = t30.[StartAvg]
FROM #TBL_EMA_RT rt
JOIN #TBL_START_AVG_1 t1 ON t1.[ShareId] = rt.[ShareId]
JOIN #TBL_START_AVG_5 t5 ON t5.[ShareId] = rt.[ShareId]
JOIN #TBL_START_AVG_10 t10 ON t10.[ShareId] = rt.[ShareId]
JOIN #TBL_START_AVG_30 t30 ON t30.[ShareId] = rt.[ShareId]

DROP TABLE #TBL_START_AVG_1
DROP TABLE #TBL_START_AVG_5
DROP TABLE #TBL_START_AVG_10
DROP TABLE #TBL_START_AVG_30

 
UPDATE #TBL_EMA_RT
	SET @Ema1 =
			CASE
			WHEN [Day#] < @Periods1 THEN NULL
            WHEN [Day#] = @Periods1 THEN rt.[AdjustedClose] * @K1 + rt.[StartingAverage1] * (1 -@K1)
            WHEN [Day#] > @Periods1 THEN rt.[AdjustedClose] * @K1 + @Ema1 * (1 - @K1)
			END
	,	@Ema5 =
			CASE
			WHEN [Day#] < @Periods5 THEN NULL
            WHEN [Day#] = @Periods5 THEN rt.[AdjustedClose] * @K5 + rt.[StartingAverage5] * (1 -@K5)
            WHEN [Day#] > @Periods5 THEN rt.[AdjustedClose] * @K5 + @Ema5 * (1 - @K5)
			END
	,	@Ema10 =
			CASE
			WHEN [Day#] < @Periods10 THEN NULL
            WHEN [Day#] = @Periods10 THEN rt.[AdjustedClose] * @K10 + rt.[StartingAverage10] * (1 -@K10)
            WHEN [Day#] > @Periods10 THEN rt.[AdjustedClose] * @K10 + @Ema10 * (1 - @K10)
			END
	,	@Ema30 =
			CASE
			WHEN [Day#] < @Periods30 THEN NULL
            WHEN [Day#] = @Periods30 THEN rt.[AdjustedClose] * @K30 + rt.[StartingAverage30] * (1 -@K30)
            WHEN [Day#] > @Periods30 THEN rt.[AdjustedClose] * @K30 + @Ema30 * (1 - @K30)
			END
	,	Ema1 = @Ema1 
	,	Ema5 = @Ema5
	,	Ema10 = @Ema10 
	,	Ema30 = @Ema30
FROM #TBL_EMA_RT rt WITH (TABLOCKX)
OPTION (MAXDOP 1)


UPDATE [ShareIndicators]
	SET [Ema1] = rt.[Ema1]
	,	[Ema5] = rt.[Ema5]
	,	[Ema10] = rt.[Ema10]
	,	[Ema30] = rt.[Ema30]
	,	[ClosedAboveEma10] = IIF(q.[AdjustedClose] > rt.[Ema10], 1, 0)
	,	[ClosedAboveEma30] = IIF(q.[AdjustedClose] > rt.[Ema30], 1, 0)
	,	[WeeklyMomentum] = rt.[Ema1]-rt.[Ema5]
FROM [ShareIndicators] si
INNER JOIN #TBL_EMA_RT rt ON rt.[ShareId] = si.[ShareId] AND rt.[Day] = si.[Day]
INNER JOIN [EodPrices] q ON q.[ShareId] = si.[ShareId] AND q.[Day] = si.[Day];


INSERT INTO [dbo].[ShareIndicators] ([Id], [ShareId], [Day], [Ema1], [Ema5], [Ema10], [Ema30], [ClosedAboveEma10], [ClosedAboveEma30], [WeeklyMomentum])
	SELECT NEWID() AS Id
		,	rt.[ShareId]
		,	rt.[Day]
		,	rt.[Ema1]
		,	rt.[Ema5]
		,	rt.[Ema10]
		,	rt.[Ema30]
		,	IIF(q.[AdjustedClose] > rt.[Ema10], 1, 0)
		,	IIF(q.[AdjustedClose] > rt.[Ema30], 1, 0)
		,	rt.[Ema1]-rt.[Ema5]
	FROM #TBL_EMA_RT rt
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = rt.[ShareId] AND si.[Day] = rt.[Day]
	LEFT JOIN [EodPrices] q ON q.[ShareId] = si.[ShareId] AND q.[Day] = si.[Day]
	WHERE si.[Id] IS NULL



DROP TABLE #TBL_EMA_RT;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;


RETURN

GO
	



