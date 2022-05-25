USE [PnFData]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerate10WeekHighLowEma]    Script Date: 13/03/2022 17:38:53 ******/
DROP PROCEDURE [dbo].[uspGenerate10WeekHighLowEma]
GO

/****** Object:  StoredProcedure [dbo].[uspGenerate10WeekHighLowEma]    Script Date: 13/03/2022 17:38:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspGenerate10WeekHighLowEma] 
	AS
SET NOCOUNT ON

RAISERROR (N'Generating 10 day high-low index moving averages ...', 0, 0) WITH NOWAIT;

IF OBJECT_ID('tempdb..#TBL_EMA_RT') IS NOT NULL BEGIN
    DROP TABLE #TBL_EMA_RT
END

IF OBJECT_ID('tempdb..#TBL_START_AVG') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG
END

-- Generate 10 day EMA (50 day periods)
DECLARE @Periods int;
SET @Periods = 10;
 
SELECT [IndexId] 
	,	ROW_NUMBER() OVER (PARTITION BY [IndexId] ORDER BY [IndexId],[Day]) AS [Day#]
	,	[Day]
	,	[PercentHighLow]
	,	CAST(NULL AS FLOAT) AS Ema
	,	CAST(NULL AS FLOAT) AS StartingAvg
INTO #TBL_EMA_RT
FROM [IndexValues]
WHERE [Value] < 1000000.00		
ORDER BY [IndexId], [Day#]

--- The large value comparison is to filter out rubbish data coming from AlphaVantage

 
CREATE UNIQUE CLUSTERED INDEX EMA_IDX_RT ON #TBL_EMA_RT (IndexId, Day#)

 
SELECT [IndexId], AVG([PercentHighLow]) AS StartingAvg 
INTO #TBL_START_AVG
FROM #TBL_EMA_RT 
WHERE [Day#] <= @Periods 
GROUP BY [IndexId]
 
-- Put the starting average in the running total table
UPDATE t1
	SET [StartingAvg] = t2.[StartingAvg]
FROM #TBL_EMA_RT t1
JOIN #TBL_START_AVG t2 ON t1.[IndexId] = t2.[IndexId]

DROP TABLE #TBL_START_AVG;


DECLARE @C FLOAT = 2.0 / (1 + @Periods) 
DECLARE @Ema FLOAT
 
UPDATE t1
	SET @Ema =
        CASE
			WHEN [Day#] < @Periods THEN NULL
            WHEN [Day#] = @Periods then t1.[PercentHighLow] * @C + t1.[StartingAvg] * (1 -@C)
            WHEN [Day#] > @Periods then t1.[PercentHighLow] * @C + @Ema * (1 - @C)
        END
    ,	Ema = @Ema 
FROM #TBL_EMA_RT t1 WITH (TABLOCKX)
OPTION (MAXDOP 1)


UPDATE iv
	SET [HighLowEma10] = t1.[Ema]
FROM [dbo].[IndexValues] iv
INNER JOIN #TBL_EMA_RT t1 ON t1.[IndexId] = iv.[IndexId] AND t1.[Day] = iv.[Day]


DROP TABLE #TBL_EMA_RT;

RAISERROR (N'Done', 0, 0) WITH NOWAIT;


RETURN

GO
	



