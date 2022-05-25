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

RAISERROR (N'Generating 10 week and 30 week share value EMAs ...', 0, 0) WITH NOWAIT;

IF OBJECT_ID('tempdb..#TBL_EMA_RT') IS NOT NULL BEGIN
    DROP TABLE #TBL_EMA_RT
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_10') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_10
END

IF OBJECT_ID('tempdb..#TBL_START_AVG_30') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG_30
END

-- Generate 10 week EMA (50 day periods)
DECLARE @Periods10 int, @Periods30 int, @K10 FLOAT, @K30 FLOAT
DECLARE @Ema10 FLOAT, @Ema30 FLOAT
SET @Periods10 = 50;
SET @Periods30 = 150;
SET @K10 = 2.0/(1+@Periods10);
SET @K30 = 2.0/(1+@Periods30);
 
SELECT [ShareId] 
	,	ROW_NUMBER() OVER (PARTITION BY [ShareId] ORDER BY [ShareId],[Day]) AS [Day#]
	,	[Day]
	,	[AdjustedClose]
	,	CAST(NULL AS FLOAT) AS StartingAverage10
	,	CAST(NULL AS FLOAT) AS StartingAverage30
	,	CAST(NULL AS FLOAT) AS Ema10
	,	CAST(NULL AS FLOAT) AS Ema30
INTO #TBL_EMA_RT
FROM [EodPrices]
WHERE [AdjustedClose] < 1000000.00		
ORDER BY [ShareId], [Day#]

--- The large value comparison is to filter out rubbish data coming from AlphaVantage

 
CREATE UNIQUE CLUSTERED INDEX EMA_IDX_RT ON #TBL_EMA_RT ([ShareId], [Day#])


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

UPDATE t1
	SET [StartingAverage10] = t10.[StartAvg]
	,	[StartingAverage30] = t30.[StartAvg]
FROM #TBL_EMA_RT t1
JOIN #TBL_START_AVG_10 t10 ON t10.[ShareId] = t1.[ShareId]
JOIN #TBL_START_AVG_30 t30 ON t30.[ShareId] = t1.[ShareId]

DROP TABLE #TBL_START_AVG_10
DROP TABLE #TBL_START_AVG_30

 
UPDATE t1
	SET @Ema10 =
			CASE
			WHEN [Day#] < @Periods10 THEN NULL
            WHEN [Day#] = @Periods10 THEN t1.[AdjustedClose] * @K10 + t1.[StartingAverage10] * (1 -@K10)
            WHEN [Day#] > @Periods10 THEN t1.[AdjustedClose] * @K10 + @Ema10 * (1 - @K10)
			END
	,	@Ema30 =
			CASE
			WHEN [Day#] < @Periods30 THEN NULL
            WHEN [Day#] = @Periods30 THEN t1.[AdjustedClose] * @K30 + t1.[StartingAverage30] * (1 -@K30)
            WHEN [Day#] > @Periods30 THEN t1.[AdjustedClose] * @K30 + @Ema30 * (1 - @K30)
			END
	,	Ema10 = @Ema10 
	,	Ema30 = @Ema30
FROM #TBL_EMA_RT t1 WITH (TABLOCKX)
OPTION (MAXDOP 1)


UPDATE si
	SET si.[Ema10] = t1.[Ema10]
	,	si.[Ema30] = t1.[Ema30]
	,	si.[ClosedAboveEma10] = IIF(q.[AdjustedClose] > t1.[Ema10], 1, 0)
	,	si.[ClosedAboveEma30] = IIF(q.[AdjustedClose] > t1.[Ema30], 1, 0)
FROM [dbo].[ShareIndicators] si
INNER JOIN #TBL_EMA_RT t1 ON t1.[ShareId] = si.[ShareId] AND t1.[Day] = si.[Day]
INNER JOIN [EodPrices] q ON q.[ShareId] = si.[ShareId] AND q.[Day] = si.[Day];


INSERT INTO [dbo].[ShareIndicators] ([Id], [ShareId], [Day], [Ema10], [Ema30], [ClosedAboveEma10], [ClosedAboveEma30])
	SELECT NEWID() AS Id
		,	t1.[ShareId]
		,	t1.[Day]
		,	t1.[Ema10]
		,	t1.[Ema30]
		,	IIF(q.[AdjustedClose] > t1.[Ema10], 1, 0)
		,	IIF(q.[AdjustedClose] > t1.[Ema30], 1, 0)
	FROM #TBL_EMA_RT t1
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = t1.[ShareId] AND si.[Day] = t1.[Day]
	LEFT JOIN [EodPrices] q ON q.[ShareId] = si.[ShareId] AND q.[Day] = si.[Day]
	WHERE si.[Id] IS NULL



DROP TABLE #TBL_EMA_RT;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;


RETURN

GO
	



