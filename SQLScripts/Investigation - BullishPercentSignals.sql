USE [PnFData]
GO


DECLARE @IsRising AS INT			= 0x0001; --       // Going up
DECLARE @IsFalling AS INT			= 0x0002; --       // Going down
DECLARE @DoubleTop AS INT			= 0x0004; --       // Double Bottom
DECLARE @DoubleBottom AS INT		= 0x0008; --       // Double Bottom
DECLARE @TripleTop AS INT			= 0x0010; --       // Triple Top
DECLARE @TripleBottom AS INT		= 0x0020; --       // Triple Bottom
DECLARE @AboveBullSupport AS INT	= 0x0040; --       // Current box is abobe bullish support level

select [Day]
	, IIF((Signals&@IsRising)=@IsRising, 1, 0) Rising
	, IIF((Signals&@DoubleTop)=@DoubleTop, 1, 0) DT
	, IIF((Signals&@TripleTop)=@TripleTop, 1, 0) TT
	, [Value]
from PnFSignals
where PnFChartId = '4335883b-8712-436b-83c9-5a45ae3c8cca'
order by [Day] DESC


SELECT [Id], [ColumnType],[Index], [CurrentBoxIndex], [BullSupportIndex],[ShowBullishSupport], [CreatedAt]
	FROM PnFColumns
	WHERE PnFChartId = '4335883b-8712-436b-83c9-5a45ae3c8cca'
	ORDER BY [Index] DESC

