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


IF OBJECT_ID('tempdb..#TBL_EMA_RT') IS NOT NULL BEGIN
    DROP TABLE #TBL_EMA_RT
END

-- Generate 10 week EMA (50 day periods)
DECLARE @Periods int;
SET @Periods = 49;
 
SELECT [ShareId], 
	ROW_NUMBER() OVER (PARTITION BY [ShareId] ORDER BY [ShareId],[Day]) AS [QuoteId],
	[Day],
	[Close],
	CAST(NULL AS FLOAT) AS Ema
INTO #TBL_EMA_RT
FROM [EodPrices]
ORDER BY [ShareId], [QuoteId]

 
CREATE UNIQUE CLUSTERED INDEX EMA_IDX_RT ON #TBL_EMA_RT (ShareId, QuoteId)

IF OBJECT_ID('tempdb..#TBL_START_AVG') IS NOT NULL BEGIN
    DROP TABLE #TBL_START_AVG
END
 
SELECT [ShareId], AVG([Close]) AS Start_Avg 
INTO #TBL_START_AVG
FROM #TBL_EMA_RT 
WHERE QuoteId <= @Periods 
GROUP BY [ShareId]
 
DECLARE @C FLOAT = 2.0 / (1 + @Periods) 
DECLARE @Ema FLOAT
 
UPDATE
    T1
SET
    @Ema =
        CASE
            WHEN QuoteId = @Periods then T2.Start_Avg
            WHEN QuoteId > @Periods then T1.[Close] * @C + @Ema * (1 - @C)
        END
    ,Ema = @Ema 
FROM
    #TBL_EMA_RT T1
JOIN
    #TBL_START_AVG T2
ON
    T1.[ShareId] = T2.[ShareId]
OPTION (MAXDOP 1)

SELECT t.[ShareId], t.[Day], CAST(Ema AS NUMERIC(10,2)) As Ema
	INTO #TBL_EMA_50
	FROM #TBL_EMA_RT AS t
	ORDER BY t.[ShareId], t.[Day] DESC;

-- Now Generate 30 week EMA (150 day periods)
SET @Periods = 149;
SET @C = 2.0 / (1 + @Periods) 
SET @Ema = NULL;

UPDATE #TBL_EMA_RT
SET [Ema] = NULL;


TRUNCATE TABLE #TBL_START_AVG

INSERT INTO #TBL_START_AVG 
SELECT [ShareId], AVG([Close]) AS Start_Avg 
	FROM #TBL_EMA_RT 
	WHERE QuoteId <= @Periods 
	GROUP BY [ShareId]
 
 
UPDATE
    T1
SET
    @Ema =
        CASE
            WHEN QuoteId = @Periods then T2.Start_Avg
            WHEN QuoteId > @Periods then T1.[Close] * @C + @Ema * (1 - @C)
        END
    ,Ema = @Ema 
FROM
    #TBL_EMA_RT T1
JOIN
    #TBL_START_AVG T2
ON
    T1.[ShareId] = T2.[ShareId]
OPTION (MAXDOP 1)

SELECT t.[ShareId], t.[Day], CAST(Ema AS NUMERIC(10,2)) As Ema
	INTO #TBL_EMA_150
	FROM #TBL_EMA_RT AS t
	ORDER BY t.[ShareId], t.[Day] DESC;

-- Gather it all together.
SELECT q.[ShareId], q.[Day], q.[Close], ema10.[Ema] As Ema10, ema30.[Ema] AS Ema30
	INTO #TBL_EMAS
	FROM [EodPrices] q
	LEFT JOIN #TBL_EMA_50 ema10 ON ema10.ShareId = q.ShareId and ema10.[Day] = q.[Day]
	LEFT JOIN #TBL_EMA_150 ema30 ON ema30.ShareId = q.ShareId and ema30.[Day] = q.[Day]
	ORDER BY q.[ShareId], q.[Day]

UPDATE si
	SET si.[Ema10] = t1.[Ema10],
		si.[Ema30] = t1.[Ema30]
FROM [dbo].[ShareIndicators] si
INNER JOIN #TBL_EMAS t1
ON t1.[ShareId] = si.[ShareId]
	AND t1.[Day] = si.[Day];


INSERT INTO [dbo].[ShareIndicators] ([Id], [ShareId], [Day], [Ema10], [Ema30])
	SELECT NEWID() AS Id,
		t1.[ShareId],
		t1.[Day],
		t1.[Ema10],
		t1.[Ema30]
	FROM #TBL_EMAS t1
	LEFT JOIN [ShareIndicators] si ON si.[ShareId] = t1.[ShareId] AND si.[Day] = t1.[Day]
	WHERE si.[Id] IS NULL



DROP TABLE #TBL_EMA_50;
DROP TABLE #TBL_EMA_150;
DROP TABLE #TBL_EMA_RT;
DROP TABLE #TBL_START_AVG;
DROP TABLE #TBL_EMAS;


RETURN

GO
	



