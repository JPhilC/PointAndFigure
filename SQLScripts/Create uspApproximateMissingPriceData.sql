USE [PnFData]
GO

DROP PROCEDURE [dbo].[uspApproximateMissingPriceData]
GO

/****** Object:  StoredProcedure [dbo].[uspApproximateMissingPriceData]    Script Date: 13/03/2022 17:38:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspApproximateMissingPriceData] 
	AS
SET NOCOUNT ON

RAISERROR (N'Searching missing prices data ...', 0, 0) WITH NOWAIT;

IF OBJECT_ID('tempdb..#Days') IS NOT NULL BEGIN
	DROP TABLE #Days;
END

IF OBJECT_ID('tempdb..#sortedPrices') IS NOT NULL BEGIN
	DROP TABLE #sortedPrices;
END

IF OBJECT_ID('tempdb..#raw') IS NOT NULL BEGIN
	DROP TABLE #raw;
END

IF OBJECT_ID('tempdb..#fixes') IS NOT NULL BEGIN
	DROP TABLE #fixes;
END

SELECT DISTINCT [Day] 
	INTO #Days
	FROM [EodPrices]
	ORDER BY [Day]

SELECT s.[Id] [ShareId], d.[Day], p.[AdjustedClose], p.[Id] [PricesId], ROW_NUMBER() OVER(PARTITION BY s.[Id] ORDER BY d.[Day] ASC) as Ordinal#
	INTO #sortedPrices 
	FROM [Shares] s
	LEFT JOIN [#Days] d ON d.[Day] IS NOT NULL
	LEFT JOIN [EodPrices] p ON p.[ShareId] = s.[Id] AND p.[Day] = d.[Day]
	ORDER BY s.[Id], d.[Day]

--SELECT * FROM #sortedPrices;

SELECT [ShareId], [Day], [PricesId], CASE WHEN [PricesId] IS NOT NULL THEN [Ordinal#] END AS [Ordinal#]
	INTO #raw
	FROM #sortedPrices
	ORDER BY [ShareId], [Day]


SELECT [ShareId], [Day], [PricesId], Relevantid,
  MAX(Relevantid) OVER(PARTITION BY [ShareId] ORDER BY [Day]
            ROWS UNBOUNDED PRECEDING ) AS grp
INTO #fixes
FROM #raw
  CROSS APPLY ( VALUES( CASE WHEN [PricesId] IS NOT NULL THEN [Ordinal#] END ) )
    AS A(relevantid);

RAISERROR (N'Creating missing prices data ...', 0, 0) WITH NOWAIT;

INSERT INTO [EodPrices] ([Id], [Day], [Open], [High], [Low], [Close], [Volume], [ShareId], [AdjustedClose], [DividendAmount], [SplitCoefficient])
SELECT NEWID() AS Id, sp.[Day], p.[Open], p.[High], p.[Low], p.[Close], p.[Volume], sp.[ShareId], p.[AdjustedClose], p.[DividendAmount], p.[SplitCoefficient]
	FROM #sortedPrices sp
	LEFT JOIN #fixes f ON f.[ShareId] = sp.ShareId AND f.[Day] = sp.[Day]
	LEFT JOIN #sortedPrices nv ON nv.[ShareId] = sp.[ShareId] AND nv.[Ordinal#] = f.[Grp]
	LEFT JOIN [EodPrices] p ON p.[Id] = nv.PricesId
	WHERE sp.[PricesId] IS NULL AND f.[Grp] IS NOT NULL



RAISERROR (N'Done. Missing data replaced with last non-null value ...', 0, 0) WITH NOWAIT;

RETURN

