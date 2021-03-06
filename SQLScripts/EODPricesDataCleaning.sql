/****** Script for SelectTopNRows command from SSMS  ******/


-- Query to get index ranges

SELECT [IndexId], MIN([Value]) AS MinValue, MAX([Value]) AS MaxValue, MAX([Value])/MIN([Value]) AS Ratio
INTO #Diffs
FROM [IndexValues]
GROUP BY [IndexId]
ORDER BY MAX([Value])/MIN([Value]) DESC


SELECT i.[ExchangeCode], i.[ExchangeSubCode], i.[SuperSector], d.[Ratio]
	FROM #Diffs d
	LEFT JOIN [Indices] i ON i.[Id] = d.[IndexId]
ORDER BY d.[Ratio] DESC



-- Query to look at Shares within an index
DROP TABLE #Index

SELECT s.[Id], s.[Tidm], q.[Close]
	INTO #Index
	FROM [Shares] s
	RIGHT OUTER JOIN [EodPrices] q ON s.[Id] = q.[ShareId]
	WHERE s.[ExchangeCode] = 'NASDAQ' AND s.[SuperSector]='Industrial Goods and Services'

SELECT q.[Tidm], q.[Id], MIN([Close]) MinClose, MAX([Close]) MaxClose, AVG([Close]) AvgClose, MAX(q.[Close])/AVG(q.[Close]) AS Ratio
	FROM #Index q
	GROUP BY q.[Tidm], q.[Id]
	ORDER BY MAX(q.[Close])/AVG(q.[Close]) DESC

-- RCRT, 14BA2672-1AE7-4BC0-815D-B328FEC34763
/*
Tidm	Id	Ratio
RCRT	14BA2672-1AE7-4BC0-815D-B328FEC34763	25025
CODA	8F4568E9-03F5-4A5D-9E0C-F29D2D7337F6	9485
AWH	2FB63A14-6B18-456E-ACB4-9F9EC9B5B07A	3381
VRME	90519BB4-0B2E-4829-B879-962C38907881	3110
YELL	0716DF73-0D43-4754-9237-DFA2911D7EB1	1968.94409937888
SPCB	E042CC60-8B96-419D-A591-94DDB8709713	1384
APDN	2688BCF9-F549-43C0-B68B-93E986DB25E5	633.206691994934
BYRN	B11AE4C8-1278-4FB3-BDE0-5140E37366BB	598
CYRX	05779AA2-4B35-4FD5-A188-A04F03F362DE	574.413793103448
HQI	80725A7A-AF05-4E9A-B7BB-DCA1A3DDD3E9	487.8
NEO	DF378FA1-76C0-4CE4-89DB-3487BC6C2E92	479.04
EXAS	465D8A32-D56B-49E1-BBA5-CE3F422CFD83	442.885714285714
CEAD	7F442C8B-2EB9-4C1B-B67F-5F6B5E961977	393.385116512152
IDEX	0AF6F524-1869-4B82-AE25-2577DBBA83CF	350
POWW	9460845C-D7B7-4B2D-8743-882359FFAB5A	326.333333333333
TOPS	2849331F-FB46-4E95-BD00-1D9FD9D441F1	269.625
METX	5C2EB826-D813-45F7-B3CB-0FDCC6AC8070	258.453038674033
DRIO	61BB8CCC-77A2-4F27-81E9-C96CEDA098EC	245.322580645161
EGLE	55E0310A-FD43-492E-95AC-1D5C9D8C3BA4	238.224299065421
GLBS	DAB49582-6423-4565-8AF5-50B6A359DE7E	237.166666666667
CEMI	3D34C094-FE07-4D4F-9259-BD0E9C4D3AB4	207.2
IVDA	2653CB78-76A6-4488-B1E6-A9914525A3D5	188.5
WHLM	7A810B9F-85C0-4707-BA75-B1A125C591CF	170.571428571429
PRPO	D3BEC2C1-2923-404B-A12A-38C6F98FEF70	164.21568627451
MVIS	03C21125-C170-4FB6-86E8-0AEBC359BE1D	155.529411764706
RDNT	F911C55C-3ED8-4978-882B-7F012ABE363C	146.730769230769
LINC	27C0AE9E-3D85-4E78-B706-C59576B2145F	143.075356415479
BWEN	0831FD90-B1E0-4BB9-94D7-3727BF9ABDB1	136.320305052431
TTOO	09E1FCCE-9A97-4E70-8D14-36857F04D07B	129.783693843594
BKYI	EB1AFBA1-F907-4838-A091-807D39F59537	125.75
ASPU	433025D7-2148-462A-A8F9-DC777A64B281	122.641509433962
CDNA	6EE844C7-DDFF-45B2-83E2-CD71A8D6CC52	121.628498727735
VTSI	014C7AFB-A314-4DC4-A938-1636B9988EC0	110.626702997275
DGLY	9B4A7049-E58D-4D63-8D8D-2CA257EBA943	107.774193548387
PSHG	EE560FD5-8C84-4680-B409-DF6F59D70736	105.651491365777
PIXY	F53E2A00-CE35-463A-A1B6-96DC53209F44	104.562737642586
RAIL	0BC39AF2-A7B1-4DCC-8C63-3D6D0F3E9403	100.73674516511
HBP	D389421C-BE59-47DB-A5A3-BF15EA96F819	100.636363636364*/

-- Query to look at incremental changes
SELECT q.[Day], q.[Close]
FROM EodPrices q
WHERE q.[ShareId] = '14BA2672-1AE7-4BC0-815D-B328FEC34763'
ORDER BY q.[Day] DESC




--- Try looking for large daily changes
DROP TABLE #sorted;

SELECT q.ShareId, q.[Day], q.[Close]
		, ROW_NUMBER() OVER(PARTITION BY q.[ShareId] ORDER BY q.[Day] ASC) as Ordinal#
	INTO #sorted
	FROM EodPrices q

DROP TABLE #results;

SELECT s.[ExchangeCode], s.[ExchangeSubCode], s.[SuperSector], s.[Tidm]
		, dp1.[Day]
		, d.[Close] [Before]
		, dp1.[Close] [After]
		, (dp1.[Close] - d.[Close])/d.[Close] DailyChange
		, s.[Id]
	INTO #results
	FROM #sorted d
	LEFT JOIN #sorted dp1 ON dp1.[ShareId] = d.[ShareId] AND dp1.[Ordinal#] = d.[Ordinal#]+1
	LEFT JOIN [Shares] s ON s.[Id] = d.[ShareId]

SELECT * 
	FROM #results
	ORDER BY ABS([DailyChange]) DESC

UPDATE [EodPrices]
	SET [AdjustedClose] = [Close]
	WHERE (ABS([AdjustedClose] - [Close])/[Close]) > 0.5

